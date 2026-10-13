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

    [Header("Referencias")]
    [SerializeField] Transform cameraTransform;

    CharacterController controller;
    PlayerInputHandler input;
    Vector3 horizontalVelocity;
    float verticalVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInputHandler>();
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        HandleMovement();
        HandleGravity();

        Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
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

    void HandleGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        else
            verticalVelocity += gravity * Time.deltaTime;
    }
}