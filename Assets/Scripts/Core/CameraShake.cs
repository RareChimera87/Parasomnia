using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [SerializeField] CinemachineCameraOffset cameraOffset;

    float strength;
    float duration;
    float timer;

    void Awake()
    {
        Instance = this;
        if (cameraOffset == null) cameraOffset = GetComponent<CinemachineCameraOffset>();
    }

       public void Shake(float newStrength, float newDuration)
    {
        Debug.Log($"Shake pedido: fuerza {newStrength}, duración {newDuration}");

        float current = timer > 0f ? strength * (timer / duration) : 0f;
        if (newStrength < current) return;

        strength = newStrength;
        duration = newDuration;
        timer = newDuration;
    }

        void Update()
    {
        if (timer > 0f)
        {
            float current = strength * Mathf.Clamp01(timer / duration);
            cameraOffset.Offset = Random.insideUnitSphere * current;

            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            timer -= dt;
        }
        else
        {
            cameraOffset.Offset = Vector3.zero;
        }
    }

        [ContextMenu("Probar temblor")]
    void TestShake()
    {
        Shake(0.3f, 0.5f);
    }
}