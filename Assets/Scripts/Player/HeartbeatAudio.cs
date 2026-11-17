using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class HeartbeatAudio : MonoBehaviour
{
    [SerializeField] float maxDistance = 15f;
    [SerializeField] float maxVolume = 1f;
    [SerializeField] float minPitch = 0.9f;
    [SerializeField] float maxPitch = 1.4f;
    [SerializeField] float smoothing = 3f;

    AudioSource source;
    ShadowEnemy[] enemies;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.loop = true;
        source.volume = 0f;
        enemies = FindObjectsByType<ShadowEnemy>(FindObjectsSortMode.None);
    }

    void Start()
    {
        source.Play();
    }

    void Update()
    {
        float closest = float.MaxValue;
        foreach (ShadowEnemy enemy in enemies)
        {
            if (enemy == null || !enemy.IsActive) continue;
            float d = Vector3.Distance(transform.position, enemy.transform.position);
            if (d < closest) closest = d;
        }

        float intensity = Mathf.Clamp01(1f - closest / maxDistance);
        if (Time.timeScale == 0f) intensity = 0f;

        float targetVolume = intensity * maxVolume;
        float targetPitch = Mathf.Lerp(minPitch, maxPitch, intensity);
        float t = smoothing * Time.unscaledDeltaTime;

        source.volume = Mathf.Lerp(source.volume, targetVolume, t);
        source.pitch = Mathf.Lerp(source.pitch, targetPitch, t);
    }
}