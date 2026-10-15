using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float rotationSpeed = 12f;

    [Header("Gravedad")]
    [SerializeField] float gravity = -25f;
    [SerializeField] float fallMultiplier = 1.5f;
    [SerializeField] float lowJumpMultiplier = 2f;

    [Header("Salto")]
    [SerializeField] float jumpHeight = 2f;
    [SerializeField] float coyoteTime = 0.15f;
    [SerializeField] float jumpBufferTime = 0.15f;

    [Header("Referencias")]
    [SerializeField] Transform cameraTransform;

    CharacterController controller;
    PlayerInputHandler input;
    Vector3 horizontalVelocity;
    float verticalVelocity;
    float coyoteTimer;
    float jumpBufferTimer;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInputHandler>();
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
        HandleGravity();

        Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        if ((controller.collisionFlags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
            verticalVelocity = 0f;
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

    public void Teleport(Vector3 position)
{
    controller.enabled = false;
    transform.position = position;
    controller.enabled = true;
    horizontalVelocity = Vector3.zero;
    verticalVelocity = 0f;
}
}