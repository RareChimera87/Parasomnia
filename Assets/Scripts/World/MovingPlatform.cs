using UnityEngine;

[DefaultExecutionOrder(-50)]
public class MovingPlatform : MonoBehaviour
{
    public enum Axis { X, Y, Z, Custom }

    [Header("Recorrido")]
    [SerializeField] Axis axis = Axis.X;
    [SerializeField] float distance = 5f;
    [SerializeField] Vector3 customOffset = new Vector3(0f, 0f, 5f);
    [SerializeField] bool useLocalAxis = false;

    [Header("Tiempo")]
    [SerializeField] float speed = 2f;
    [SerializeField] float waitTime = 1f;
    [SerializeField] float startDelay = 0f;

    // Cuánto se movió este frame (lo usa PlayerMovement para arrastrar al jugador)
    public Vector3 Delta { get; private set; }

    enum Phase { Waiting, Moving }

    Vector3 startPos;
    Vector3 endPos;
    Phase phase = Phase.Waiting;
    float timer;
    bool goingToEnd = true;

    void Start()
    {
        startPos = transform.position;
        endPos = startPos + GetOffset();
        timer = startDelay;
    }

    void Update()
    {
        Delta = Vector3.zero;

        if (phase == Phase.Waiting)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f) phase = Phase.Moving;
            return;
        }

        Vector3 target = goingToEnd ? endPos : startPos;
        Vector3 before = transform.position;

        Vector3 next = Vector3.MoveTowards(before, target, speed * Time.deltaTime);
        transform.position = next;
        Delta = next - before;

        // Llegó al destino (con margen por el error de decimales): espera y cambia de dirección
        if ((next - target).sqrMagnitude < 0.0001f)
        {
            goingToEnd = !goingToEnd;
            phase = Phase.Waiting;
            timer = waitTime;
        }
    }

    Vector3 GetOffset()
    {
        Vector3 offset = axis switch
        {
            Axis.X => Vector3.right * distance,
            Axis.Y => Vector3.up * distance,
            Axis.Z => Vector3.forward * distance,
            _ => customOffset
        };
        return useLocalAxis ? transform.TransformDirection(offset) : offset;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 a = Application.isPlaying ? startPos : transform.position;
        Vector3 b = Application.isPlaying ? endPos : a + GetOffset();
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawWireSphere(b, 0.3f);
    }
}