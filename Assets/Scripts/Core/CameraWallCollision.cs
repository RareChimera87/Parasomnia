using UnityEngine;
using Unity.Cinemachine;

// Extensión de Cinemachine: si hay una pared entre el jugador y la cámara,
// acerca la cámara hasta justo antes de la pared.
public class CameraWallCollision : CinemachineExtension
{
    [Tooltip("Capas que la cámara no puede atravesar")]
    [SerializeField] LayerMask collideAgainst = 1; // Default
    [Tooltip("Distancia que deja entre la cámara y la pared")]
    [SerializeField] float cameraRadius = 0.3f;
    [Tooltip("Lo más cerca que puede quedar la cámara del jugador")]
    [SerializeField] float minDistance = 0.5f;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        // Se aplica después de calcular la posición, antes de calcular hacia dónde mira
        if (stage != CinemachineCore.Stage.Body) return;

        Transform target = vcam.Follow;
        if (target == null) return;

        Vector3 from = target.position;
        Vector3 camPos = state.GetCorrectedPosition();
        Vector3 dir = camPos - from;
        float distance = dir.magnitude;
        if (distance < 0.001f) return;
        dir /= distance;

        if (Physics.Raycast(from, dir, out RaycastHit hit, distance + cameraRadius,
                            collideAgainst, QueryTriggerInteraction.Ignore))
        {
            float safeDistance = Mathf.Max(hit.distance - cameraRadius, minDistance);
            Vector3 newPos = from + dir * safeDistance;
            state.PositionCorrection += newPos - camPos;
        }
    }
}