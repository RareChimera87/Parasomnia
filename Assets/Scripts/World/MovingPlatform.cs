using UnityEngine;

[DefaultExecutionOrder(-50)]
public class MovingPlatform : MonoBehaviour
{
    public enum Axis { X, Y, Z, Custom }

    [Header("Recorrido")]
    [SerializeField] Axis axis = Axis.Z;
    [SerializeField] float distance = 5f;
    [SerializeField] Vector3 customOffset = new Vector3(0f, 0f, 5f);
    [SerializeField] bool useLocalAxis = false;

    [Header("Tiempo")]
    [SerializeField] float speed = 2f;
    [SerializeField] float waitTime = 1f;
    [SerializeField] float startDelay = 0f;

    public Vector3 Delta { get; private set; }

    Vector3 startPos;
    Vector3 endPos;
    Vector3 target;
    float waitTimer;

    void Start()
    {
        startPos = transform.position;
        endPos = startPos + GetOffset();
        target = endPos;
        waitTimer = startDelay;
    }

    void Update()
    {
        Vector3 previous = transform.position;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

            if (transform.position == target)
            {
                target = (target == endPos) ? startPos : endPos;
                waitTimer = waitTime;
            }
        }

        Delta = transform.position - previous;
    }

    Vector3 GetOffset()
    {
        Vector3 offset;
        switch (axis)
        {
            case Axis.X: offset = Vector3.right * distance;   break;
            case Axis.Y: offset = Vector3.up * distance;      break;
            case Axis.Z: offset = Vector3.forward * distance; break;
            default:     offset = customOffset;               break;
        }

        return useLocalAxis ? transform.TransformDirection(offset) : offset;
    }

    void OnDrawGizmos()
    {
        Vector3 from = Application.isPlaying ? startPos : transform.position;
        Vector3 to = Application.isPlaying ? endPos : transform.position + GetOffset();

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(from, to);
        Gizmos.DrawWireSphere(to, 0.3f);
    }
}