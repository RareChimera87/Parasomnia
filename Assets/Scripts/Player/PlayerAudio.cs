using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerAudio : MonoBehaviour
{
    [Header("Fuentes")]
    [SerializeField] AudioSource stepsSource;
    [SerializeField] AudioSource sfxSource;

    [Header("Pasos")]
    [SerializeField] AudioClip walkClip;
    [SerializeField] AudioClip runClip;
    [SerializeField] float runThreshold = 4.8f;
    [SerializeField] float minMoveSpeed = 0.5f;

    [Header("Salto y aterrizaje")]
    [SerializeField] AudioClip jumpClip;
    [SerializeField] AudioClip landClip;
    [SerializeField] float minLandSpeed = 4f;

    CharacterController controller;
    PlayerMovement movement;
    bool wasGrounded = true;
    float maxFallSpeed;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        movement = GetComponent<PlayerMovement>();
        stepsSource.loop = true;
        stepsSource.playOnAwake = false;
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            if (stepsSource.isPlaying) stepsSource.Pause();
            return;
        }

        Vector3 velocity = controller.velocity;
        float speed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
        bool grounded = controller.isGrounded;
        bool climbing = movement != null && movement.IsClimbing;

        UpdateSteps(speed, grounded && !climbing);

        if (wasGrounded && !grounded && velocity.y > 1f)
            Play(jumpClip);

        if (!grounded)
            maxFallSpeed = Mathf.Min(maxFallSpeed, velocity.y);

        if (grounded && !wasGrounded)
        {
            if (maxFallSpeed < -minLandSpeed)
                Play(landClip);
            maxFallSpeed = 0f;
        }

        wasGrounded = grounded;
    }

    void UpdateSteps(float speed, bool canStep)
    {
        AudioClip target = null;
        if (canStep && speed > minMoveSpeed)
            target = speed > runThreshold ? runClip : walkClip;

        if (target == null)
        {
            if (stepsSource.isPlaying) stepsSource.Pause();
            return;
        }

        if (stepsSource.clip != target)
        {
            stepsSource.clip = target;
            stepsSource.Play();
        }
        else if (!stepsSource.isPlaying)
        {
            stepsSource.UnPause();
        }
    }

    void Play(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip);
    }
}