using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerInputHandler))]
public class Flashlight : MonoBehaviour
{
    [SerializeField] Light spotLight;

    [Header("Batería")]
    [SerializeField] float maxBattery = 100f;
    [SerializeField] float drainPerSecond = 5f;
    [SerializeField] float lowBatteryThreshold = 0.2f;

    public UnityEvent<float> OnBatteryChanged;

    public bool IsOn { get; private set; }
    public float Battery01 => battery / maxBattery;

    PlayerInputHandler input;
    float battery;
    float baseIntensity;

    void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        battery = maxBattery;
        baseIntensity = spotLight.intensity;
        spotLight.enabled = false;
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
    }

    public void Recharge(float amount)
    {
        battery = Mathf.Min(battery + amount, maxBattery);
        OnBatteryChanged?.Invoke(Battery01);
    }
}