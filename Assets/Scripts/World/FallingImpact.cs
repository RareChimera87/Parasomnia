using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallingImpact : MonoBehaviour
{
    [Header("Temblor al caer")]
    [SerializeField] float rumbleStrength = 0.05f;
    [SerializeField] float rumbleDuration = 1f;

    [Header("Golpe al impactar")]
    [SerializeField] float impactStrength = 0.3f;
    [SerializeField] float impactDuration = 0.5f;
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
            ShakeCamera(rumbleStrength, rumbleDuration);
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

        ShakeCamera(impactStrength, impactDuration);
    }

        void ShakeCamera(float strength, float duration)
    {
        if (CameraShake.Instance == null)
        {
            Debug.Log("FallingImpact: no hay CameraShake en la escena");
            return;
        }

        float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        float falloff = Mathf.Clamp01(1f - distance / maxDistance);
        Debug.Log($"FallingImpact: temblor {strength} a {distance:F1} m → fuerza final {strength * falloff:F2}");

        if (falloff > 0f)
            CameraShake.Instance.Shake(strength * falloff, duration);
    }
}