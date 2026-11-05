using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] float walkSpeed = 3.5f;
    [SerializeField] float sprintSpeed = 6f;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float rotationSpeed = 12f;

    [Header("Gravedad")]
    [SerializeField] float gravity = -25f;
    [SerializeField] float fallMultiplier = 1.5f;
    [SerializeField] float lowJumpMultiplier = 2f;

    [Header("Salto")]
    [SerializeField] float jumpHeight = 1.5f;
    [SerializeField] float coyoteTime = 0.15f;
    [SerializeField] float jumpBufferTime = 0.15f;

    [Header("Escalar")]                                   // NUEVO
    [SerializeField] float climbSpeed = 2.5f;
    [SerializeField] float climbCheckDistance = 0.6f;
    [SerializeField] float climbCheckHeight = 0.65f;
    [SerializeField] float wallJumpPush = 4f;
    [SerializeField] float ledgeHopHeight = 0.9f;
    [SerializeField] float regrabDelay = 0.3f;

    [Header("Referencias")]
    [SerializeField] Transform cameraTransform;

    public bool IsClimbing { get; private set; }          // NUEVO

    CharacterController controller;
    PlayerInputHandler input;
    Vector3 horizontalVelocity;
    float verticalVelocity;
    float coyoteTimer;
    float jumpBufferTimer;
    MovingPlatform currentPlatform;
    bool hitHazard;
    Vector3 wallNormal;                                   // NUEVO
    float regrabTimer;                                    // NUEVO

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInputHandler>();
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (currentPlatform != null)
            controller.Move(currentPlatform.Delta);
        currentPlatform = null;

        regrabTimer -= Time.deltaTime;                    // NUEVO

        if (IsClimbing)                                   // NUEVO
        {
            HandleClimbing();
        }
        else
        {
            if (TryStartClimb()) return;

            HandleMovement();
            HandleJump();
            HandleGravity();

            Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
            controller.Move(velocity * Time.deltaTime);

            if ((controller.collisionFlags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
                verticalVelocity = 0f;
        }

        if (hitHazard)
        {
            hitHazard = false;
            GameManager.Instance.PlayerDied();
        }
    }

    void HandleMovement()
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector2 move = input.MoveInput;
        Vector3 direction = camForward * move.y + camRight * move.x;
        if (direction.sqrMagnitude > 1f) direction.Normalize();

        float speed = input.SprintHeld ? sprintSpeed : walkSpeed;
        Vector3 targetVelocity = direction * speed;
        horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetVelocity, acceleration * Time.deltaTime);

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void HandleJump()
    {
        if (controller.isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        if (input.JumpPressed)
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }
    }

    void HandleGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
            return;
        }

        float multiplier = 1f;
        if (verticalVelocity < 0f)
            multiplier = fallMultiplier;
        else if (!input.JumpHeld)
            multiplier = lowJumpMultiplier;

        verticalVelocity += gravity * multiplier * Time.deltaTime;
    }

    // ---------- Escalar (NUEVO) ----------

    bool FindClimbableWall(out RaycastHit hit)
    {
        Vector3 origin = transform.position + Vector3.up * climbCheckHeight;
        if (Physics.Raycast(origin, transform.forward, out hit, climbCheckDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            bool isWall = Mathf.Abs(hit.normal.y) < 0.3f;
            bool climbable = hit.collider.GetComponentInParent<Climbable>() != null;
            return isWall && climbable;
        }
        return false;
    }

    bool TryStartClimb()
    {
        if (regrabTimer > 0f) return false;
        if (input.MoveInput.sqrMagnitude < 0.25f) return false;
        if (!FindClimbableWall(out RaycastHit hit)) return false;

        IsClimbing = true;
        wallNormal = hit.normal;
        horizontalVelocity = Vector3.zero;
        verticalVelocity = 0f;
        transform.rotation = Quaternion.LookRotation(-wallNormal);
        return true;
    }

    void HandleClimbing()
    {
        // Saltar hacia atrás para soltarse
        if (input.JumpPressed)
        {
            StopClimbing();
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity) * 0.8f;
            horizontalVelocity = wallNormal * wallJumpPush;
            transform.rotation = Quaternion.LookRotation(wallNormal);
            return;
        }

        Vector2 move = input.MoveInput;

        // Se acabó la pared: si iba subiendo, impulso para quedar encima del borde
        if (!FindClimbableWall(out RaycastHit hit))
        {
            StopClimbing();
            if (move.y > 0.1f)
            {
                verticalVelocity = Mathf.Sqrt(ledgeHopHeight * -2f * gravity);
                horizontalVelocity = -wallNormal * 2f;
            }
            return;
        }

        wallNormal = hit.normal;
        transform.rotation = Quaternion.LookRotation(-wallNormal);

        // Bajó hasta el suelo: se suelta
        if (controller.isGrounded && move.y < -0.1f)
        {
            StopClimbing();
            return;
        }

        Vector3 right = Vector3.Cross(Vector3.up, -wallNormal);
        Vector3 climbMove = (Vector3.up * move.y + right * move.x) * climbSpeed;
        Vector3 stickToWall = -wallNormal * 0.5f;

        controller.Move((climbMove + stickToWall) * Time.deltaTime);
    }

    void StopClimbing()
    {
        IsClimbing = false;
        regrabTimer = regrabDelay;
        coyoteTimer = 0f;
    }

    // ---------- Colisiones y utilidades ----------

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.GetComponentInParent<Hazard>() != null)
            hitHazard = true;

        if (hit.normal.y > 0.5f)
        {
            currentPlatform = hit.collider.GetComponentInParent<MovingPlatform>();

            FallingPlatform fallingPlatform = hit.collider.GetComponentInParent<FallingPlatform>();
            if (fallingPlatform != null)
                fallingPlatform.Trigger();
        }
    }

    public void Teleport(Vector3 position)
    {
        IsClimbing = false;                               // NUEVO
        controller.enabled = false;
        transform.position = position;
        controller.enabled = true;
        horizontalVelocity = Vector3.zero;
        verticalVelocity = 0f;
    }

    void OnDrawGizmosSelected()                           // NUEVO
    {
        Gizmos.color = Color.cyan;
        Vector3 origin = transform.position + Vector3.up * climbCheckHeight;
        Gizmos.DrawLine(origin, origin + transform.forward * climbCheckDistance);
    }
}