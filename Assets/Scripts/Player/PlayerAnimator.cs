using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] float groundedGrace = 0.15f;
    [SerializeField] float speedDamping = 0.1f;

    [Header("Cabeza")]
    [SerializeField] float headLift = 0f;

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
    static readonly int GroundedHash = Animator.StringToHash("Grounded");
    static readonly int ClimbingHash = Animator.StringToHash("Climbing");
    static readonly int JumpHash = Animator.StringToHash("Jump");

    CharacterController controller;
    PlayerMovement movement;
    Transform head;
    float lastGroundedTime;
    bool wasGroundedRaw = true;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        movement = GetComponent<PlayerMovement>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        head = animator.GetBoneTransform(HumanBodyBones.Head);
    }

    void Update()
    {
        Vector3 velocity = controller.velocity;
        float horizontalSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;

        bool groundedRaw = controller.isGrounded;
        if (groundedRaw) lastGroundedTime = Time.time;
        bool grounded = Time.time - lastGroundedTime < groundedGrace;

        if (wasGroundedRaw && !groundedRaw && velocity.y > 1f)
            animator.SetTrigger(JumpHash);
        wasGroundedRaw = groundedRaw;

        animator.SetFloat(SpeedHash, horizontalSpeed, speedDamping, Time.deltaTime);
        animator.SetFloat(VerticalSpeedHash, velocity.y);
        animator.SetBool(GroundedHash, grounded);
        animator.SetBool(ClimbingHash, movement != null && movement.IsClimbing);
    }

    void LateUpdate()
    {
        if (head != null && headLift != 0f)
            head.localRotation *= Quaternion.Euler(-headLift, 0f, 0f);
    }
}