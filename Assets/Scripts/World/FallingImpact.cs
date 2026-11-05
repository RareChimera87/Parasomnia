using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallingImpact : MonoBehaviour
{
    [Header("Temblor al caer")]
    [SerializeField] float rumbleAmplitude = 0.4f;
    [SerializeField] float rumbleDuration = 1f;

    [Header("Golpe al impactar")]
    [SerializeField] float impactAmplitude = 3f;
    [SerializeField] float impactDuration = 0.6f;
    [SerializeField] float minImpactSpeed = 3f;

    [Header("Distancia")]
    [SerializeField] float maxDistance = 25f;

    Rigidbody rb;
    bool falling;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!falling && !rb.isKinematic)
        {
            falling = true;
            ShakeCamera(rumbleAmplitude, rumbleDuration);
        }
        else if (falling && rb.isKinematic)
        {
            falling = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!falling) return;
        if (collision.relativeVelocity.magnitude < minImpactSpeed) return;

        ShakeCamera(impactAmplitude, impactDuration);
    }

    void ShakeCamera(float amplitude, float duration)
    {
        float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        float strength = Mathf.Clamp01(1f - distance / maxDistance);

        if (strength > 0f)
            CameraShake.Instance.Shake(amplitude * strength, duration);
    }
}