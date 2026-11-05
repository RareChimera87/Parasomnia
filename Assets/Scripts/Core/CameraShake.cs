using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [SerializeField] CinemachineBasicMultiChannelPerlin noise;

    Coroutine current;

    void Awake()
    {
        Instance = this;
        noise.AmplitudeGain = 0f;
    }

    public void Shake(float amplitude, float duration)
    {
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(ShakeRoutine(amplitude, duration));
    }

    IEnumerator ShakeRoutine(float amplitude, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            noise.AmplitudeGain = Mathf.Lerp(amplitude, 0f, t / duration);
            yield return null;
        }
        noise.AmplitudeGain = 0f;
    }
}