using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerInputHandler))]
public class Flashlight : MonoBehaviour
{
    [SerializeField] Light spotLight;
    [SerializeField] Light[] extraLights;
    [SerializeField] LayerMask obstacleMask = ~0;

    [Header("Batería")]
    [SerializeField] float maxBattery = 100f;
    [SerializeField] float drainPerSecond = 5f;
    [SerializeField] float lowBatteryThreshold = 0.2f;
    [SerializeField] bool debugLight = false;
    [Header("Lente")]
    [SerializeField] Renderer lensRenderer;
    [SerializeField] Color lensColor = new Color(0.95f, 0.9f, 0.72f);
    [SerializeField] float lensIntensity = 3f;

    public UnityEvent<float> OnBatteryChanged;

    public bool IsOn { get; private set; }
    public float Battery01 => battery / maxBattery;

    PlayerInputHandler input;
    float battery;
    float baseIntensity;
    Material lensMaterial;
    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        battery = maxBattery;
        baseIntensity = spotLight.intensity;
        if (lensRenderer != null)
        {
            lensMaterial = lensRenderer.material;
            lensMaterial.EnableKeyword("_EMISSION");
        }
        SetOn(false);
    }

    void Start()
    {
        OnBatteryChanged?.Invoke(Battery01);
    }

    void Update()
    {
        if (input.FlashlightPressed)
            Toggle();

        if (!IsOn) return;

        battery = Mathf.Max(battery - drainPerSecond * Time.deltaTime, 0f);
        OnBatteryChanged?.Invoke(Battery01);

        if (battery <= 0f)
        {
            SetOn(false);
            return;
        }

        if (Battery01 < lowBatteryThreshold)
            spotLight.intensity = baseIntensity * Random.Range(0.4f, 1f);
        else
            spotLight.intensity = baseIntensity;

        SetLensGlow(spotLight.intensity / baseIntensity);
    }

    void Toggle()
    {
        if (!IsOn && battery <= 0f) return;
        SetOn(!IsOn);
    }

    void SetOn(bool on)
    {
        IsOn = on;
        spotLight.enabled = on;
        foreach (Light l in extraLights)
            if (l != null) l.enabled = on;
        SetLensGlow(on ? 1f : 0f);
    }

    public void Recharge(float amount)
    {
        battery = Mathf.Min(battery + amount, maxBattery);
        OnBatteryChanged?.Invoke(Battery01);
    }

    public bool IsIlluminating(Vector3 point)
    {
        if (!IsOn) return false;

        Vector3 origin = spotLight.transform.position;
        Vector3 toPoint = point - origin;

        if (toPoint.magnitude > spotLight.range)
        {
            if (debugLight) Debug.Log($"Luz: fuera de alcance ({toPoint.magnitude:F1} m > {spotLight.range} m)");
            return false;
        }

        float angle = Vector3.Angle(spotLight.transform.forward, toPoint);
        if (angle > spotLight.spotAngle * 0.5f)
        {
            if (debugLight) Debug.Log($"Luz: fuera del cono ({angle:F0}° > {spotLight.spotAngle * 0.5f:F0}°)");
            return false;
        }

        if (Physics.Linecast(origin, point, out RaycastHit hit, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            if (debugLight) Debug.Log($"Luz: bloqueada por {hit.collider.name}");
            if (debugLight) Debug.DrawLine(origin, hit.point, Color.red);
            return false;
        }

        if (debugLight) Debug.Log("Luz: ILUMINADO");
        if (debugLight) Debug.DrawLine(origin, point, Color.green);
        return true;
    }
    void SetLensGlow(float amount)
    {
        if (lensMaterial == null) return;
        lensMaterial.SetColor(EmissionColorId, lensColor * lensIntensity * amount);
    }
}