using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [Header("Temblor")]
    [SerializeField] float shakeTime = 1f;
    [SerializeField] float shakeAmount = 0.05f;
    [Tooltip("Lo que tiembla (el modelo). Si se deja vacío, tiembla este objeto")]
    [SerializeField] Transform visual;

    [Header("Caída")]
    [SerializeField] float fallGravity = 20f;
    [SerializeField] float fallTime = 3f;

    [Header("Reaparecer")]
    [SerializeField] bool respawn = true;
    [SerializeField] float respawnTime = 4f;

    [Header("Sonido")]
    [SerializeField] AudioSource audioSource;
    [Tooltip("Suena mientras tiembla")]
    [SerializeField] AudioClip shakeClip;
    [Tooltip("Suena cuando se suelta y cae")]
    [SerializeField] AudioClip fallClip;

    Vector3 startPos;
    Vector3 visualStartLocal;
    Collider[] colliders;
    Renderer[] renderers;
    bool triggered;

    void Awake()
    {
        startPos = transform.position;
        if (visual == null) visual = transform;
        visualStartLocal = visual.localPosition;
        colliders = GetComponentsInChildren<Collider>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    // La llama PlayerMovement cuando el jugador se para encima
    public void Trigger()
    {
        if (triggered) return;
        triggered = true;
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        // 1. Tiembla cada vez más fuerte
        PlaySound(shakeClip);
        float t = 0f;
        while (t < shakeTime)
        {
            t += Time.deltaTime;
            float amount = shakeAmount * (t / shakeTime);
            visual.localPosition = visualStartLocal + Random.insideUnitSphere * amount;
            yield return null;
        }
        visual.localPosition = visualStartLocal;

        // 2. Se suelta y cae acelerando
        if (audioSource != null) audioSource.Stop();
        PlaySound(fallClip);

        float speed = 0f;
        t = 0f;
        while (t < fallTime)
        {
            t += Time.deltaTime;
            speed += fallGravity * Time.deltaTime;
            transform.position += Vector3.down * speed * Time.deltaTime;
            yield return null;
        }

        // 3. Desaparece
        SetVisible(false);
        if (!respawn) yield break;

        // 4. Reaparece en su lugar
        yield return new WaitForSeconds(respawnTime);
        transform.position = startPos;
        SetVisible(true);
        triggered = false;
    }

    void SetVisible(bool visible)
    {
        foreach (Renderer r in renderers) r.enabled = visible;
        foreach (Collider c in colliders) c.enabled = visible;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }
}