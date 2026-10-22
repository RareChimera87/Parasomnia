using UnityEngine;
using UnityEngine.UI;

public class BatteryUI : MonoBehaviour
{
    [SerializeField] Flashlight flashlight;
    [SerializeField] Image fillImage;
    [SerializeField] Color normalColor = Color.yellow;
    [SerializeField] Color lowColor = Color.red;
    [SerializeField] float lowThreshold = 0.2f;

    void Start()
    {
        flashlight.OnBatteryChanged.AddListener(UpdateBar);
        UpdateBar(flashlight.Battery01);
    }

    void OnDestroy()
    {
        if (flashlight != null)
            flashlight.OnBatteryChanged.RemoveListener(UpdateBar);
    }

    void UpdateBar(float value)
    {
        fillImage.fillAmount = value;
        fillImage.color = value < lowThreshold ? lowColor : normalColor;
    }
}