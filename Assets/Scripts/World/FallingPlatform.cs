using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [Header("Temblor")]
    [SerializeField] float shakeTime = 1f;
    [SerializeField] float shakeAmount = 0.05f;
    [SerializeField] Transform visual;

    [Header("Caída")]
    [SerializeField] float fallGravity = 20f;
    [SerializeField] float fallTime = 3f;

    [Header("Reaparecer")]
    [SerializeField] bool respawn = true;
    [SerializeField] float respawnTime = 4f;

    Vector3 startPos;
    Vector3 visualStartLocalPos;
    Collider[] colliders;
    Renderer[] renderers;
    bool triggered;

    void Awake()
    {
        startPos = transform.position;
        if (visual == null) visual = transform;
        visualStartLocalPos = visual.localPosition;
        colliders = GetComponentsInChildren<Collider>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    public void Trigger()
    {
        if (triggered) return;
        triggered = true;
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        // 1. Temblar, cada vez más fuerte
        float t = 0f;
        while (t < shakeTime)
        {
            t += Time.deltaTime;
            float intensity = shakeAmount * (t / shakeTime);
            visual.localPosition = visualStartLocalPos + Random.insideUnitSphere * intensity;
            yield return null;
        }
        visual.localPosition = visualStartLocalPos;

        // 2. Caer acelerando
        float speed = 0f;
        t = 0f;
        while (t < fallTime)
        {
            t += Time.deltaTime;
            speed += fallGravity * Time.deltaTime;
            transform.position += Vector3.down * speed * Time.deltaTime;
            yield return null;
        }

        SetVisible(false);

        // 3. Reaparecer
        if (!respawn) yield break;

        yield return new WaitForSeconds(respawnTime);
        transform.position = startPos;
        SetVisible(true);
        triggered = false;
    }

    void SetVisible(bool visible)
    {
        foreach (Collider c in colliders) c.enabled = visible;
        foreach (Renderer r in renderers) r.enabled = visible;
    }
}