using UnityEngine;

public class EyeWatcher : MonoBehaviour
{
    [Header("Partes")]
    [Tooltip("Las piezas que giran juntas: ojo_interno y pupila")]
    [SerializeField] Transform[] movingParts;
    [Tooltip("La pupila: sirve para saber hacia dónde mira el ojo al inicio")]
    [SerializeField] Renderer pupil;
    [Tooltip("El cristalino: sirve para encontrar el centro del ojo")]
    [SerializeField] Renderer centerReference;

    [Header("Movimiento")]
    [SerializeField] float turnSpeed = 4f;
    [Tooltip("Cuánto puede girar como máximo desde su posición original (grados)")]
    [SerializeField] float maxAngle = 70f;
    [Tooltip("Altura del jugador a la que mira (0.8 = más o menos la cara del niño)")]
    [SerializeField] float targetHeight = 0.8f;
    [SerializeField] bool activeFromStart = false;

    Transform player;
    Vector3 center;
    Vector3 restForward;
    Vector3[] offsets;
    Quaternion[] startRotations;
    Quaternion current = Quaternion.identity;
    bool active;

    void Start()
    {
        // El ojo vive en un prefab, así que busca al jugador al empezar
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;

        center = centerReference != null ? centerReference.bounds.center : transform.position;
        restForward = (pupil.bounds.center - center).normalized;

        offsets = new Vector3[movingParts.Length];
        startRotations = new Quaternion[movingParts.Length];
        for (int i = 0; i < movingParts.Length; i++)
        {
            offsets[i] = movingParts[i].position - center;
            startRotations[i] = movingParts[i].rotation;
        }

        active = activeFromStart;
    }

    public void Activate() => active = true;

    void LateUpdate()
    {
        Quaternion target = Quaternion.identity;

        if (active && player != null)
        {
            Vector3 toPlayer = (player.position + Vector3.up * targetHeight - center).normalized;
            // Limita el giro para que la pupila no se vaya a la parte de atrás del ojo
            Vector3 dir = Vector3.RotateTowards(restForward, toPlayer, maxAngle * Mathf.Deg2Rad, 0f);
            target = Quaternion.FromToRotation(restForward, dir);
        }

        // Giro suave
        current = Quaternion.Slerp(current, target, 1f - Mathf.Exp(-turnSpeed * Time.deltaTime));

        // Gira todas las piezas alrededor del centro del ojo
        for (int i = 0; i < movingParts.Length; i++)
        {
            movingParts[i].position = center + current * offsets[i];
            movingParts[i].rotation = current * startRotations[i];
        }
    }

    void OnDrawGizmosSelected()
    {
        if (centerReference == null || pupil == null) return;
        Vector3 c = centerReference.bounds.center;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(c, c + (pupil.bounds.center - c).normalized * 2f);
    }
}