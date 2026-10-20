using UnityEngine;

[DefaultExecutionOrder(-50)]
public class MovingPlatform : MonoBehaviour
{
    [SerializeField] Vector3 moveOffset = new Vector3(0f, 0f, 5f);
    [SerializeField] float speed = 2f;
    [SerializeField] float waitTime = 1f;

    public Vector3 Delta { get; private set; }

    Vector3 startPos;
    Vector3 endPos;
    Vector3 target;
    float waitTimer;

    void Start()
    {
        startPos = transform.position;
        endPos = startPos + moveOffset;
        target = endPos;
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

    void OnDrawGizmos()
    {
        Vector3 from = Application.isPlaying ? startPos : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(from, from + moveOffset);
        Gizmos.DrawWireSphere(from + moveOffset, 0.3f);
    }
}