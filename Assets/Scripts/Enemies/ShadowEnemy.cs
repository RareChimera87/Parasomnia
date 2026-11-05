using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ShadowEnemy : MonoBehaviour
{
    enum State { Idle, Chasing, Stunned, Banished, Returning }

    [Header("Referencias")]
    [SerializeField] GameObject visual;
    [SerializeField] Transform player;
    [SerializeField] Flashlight flashlight;

    [Header("Detección")]
    [SerializeField] float detectionRadius = 12f;
    [SerializeField] float giveUpRadius = 25f;

    [Header("Movimiento")]
    [SerializeField] float chaseSpeed = 4f;
    [SerializeField] float retreatSpeed = 2f;
    [SerializeField] float returnSpeed = 3f;
     [SerializeField] float targetHeight = 0.8f;

    [Header("Luz")]
    [SerializeField] float timeToBanish = 2f;
    [SerializeField] float respawnTime = 8f;
    [SerializeField] float shakeAmount = 0.08f;

    [Header("Debug")]
    [SerializeField] State state = State.Idle;

    Vector3 homePosition;
    float lightTimer;
    float banishTimer;
    Collider[] colliders;

    float DistanceToPlayer => Vector3.Distance(transform.position, player.position);
    bool IsLit => flashlight.IsIlluminating(transform.position);

    void Awake()
    {
        homePosition = transform.position;
        colliders = GetComponentsInChildren<Collider>();
        GetComponent<Rigidbody>().isKinematic = true;

        if (player == null) player = GameObject.FindWithTag("Player").transform;
        if (flashlight == null) flashlight = player.GetComponent<Flashlight>();
    }

    void Start()
    {
        GameManager.Instance.OnPlayerRespawned.AddListener(ResetEnemy);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerRespawned.RemoveListener(ResetEnemy);
    }

    void Update()
    {
        switch (state)
        {
            case State.Idle:      UpdateIdle();      break;
            case State.Chasing:   UpdateChasing();   break;
            case State.Stunned:   UpdateStunned();   break;
            case State.Banished:  UpdateBanished();  break;
            case State.Returning: UpdateReturning(); break;
        }
    }

    // ---------- Estados ----------

    void UpdateIdle()
    {
        if (DistanceToPlayer <= detectionRadius)
            SetState(State.Chasing);
    }

    void UpdateChasing()
    {
        if (IsLit)
        {
            SetState(State.Stunned);
            return;
        }

        if (DistanceToPlayer > giveUpRadius)
        {
            SetState(State.Returning);
            return;
        }

        lightTimer = Mathf.Max(lightTimer - Time.deltaTime * 0.5f, 0f);
        MoveTowards(player.position + Vector3.up * targetHeight, chaseSpeed);
    }

    void UpdateStunned()
    {
        if (!IsLit)
        {
            SetState(State.Chasing);
            return;
        }

        lightTimer += Time.deltaTime;

        Vector3 away = (transform.position - player.position).normalized;
        transform.position += away * retreatSpeed * Time.deltaTime;

        visual.transform.localPosition = Random.insideUnitSphere * shakeAmount;

        if (lightTimer >= timeToBanish)
            Banish();
    }

    void UpdateBanished()
    {
        banishTimer -= Time.deltaTime;
        if (banishTimer <= 0f)
            ResetEnemy();
    }

    void UpdateReturning()
    {
        if (DistanceToPlayer <= detectionRadius)
        {
            SetState(State.Chasing);
            return;
        }

        MoveTowards(homePosition, returnSpeed);

        if (transform.position == homePosition)
            SetState(State.Idle);
    }

    // ---------- Acciones ----------

    void MoveTowards(Vector3 target, float speed)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        Vector3 direction = target - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    void Banish()
    {
        lightTimer = 0f;
        banishTimer = respawnTime;
        SetVisible(false);
        SetState(State.Banished);
    }

    void ResetEnemy()
    {
        transform.position = homePosition;
        lightTimer = 0f;
        SetVisible(true);
        SetState(State.Idle);
    }

    void SetVisible(bool visible)
    {
        visual.SetActive(visible);
        foreach (Collider c in colliders)
            c.enabled = visible;
    }

    void SetState(State newState)
    {
        state = newState;
        visual.transform.localPosition = Vector3.zero;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, giveUpRadius);
    }
}