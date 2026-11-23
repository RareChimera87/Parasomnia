using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ShadowEnemy : MonoBehaviour
{
    public enum State { Idle, Chasing, Stunned, Banished, Returning }
    public enum ActivationMode { Proximity, Light, Sight }

    [Header("Referencias")]
    [SerializeField] GameObject visual;
    [SerializeField] Transform player;
    [SerializeField] Flashlight flashlight;

    [Header("Activación")]
    [SerializeField] ActivationMode activation = ActivationMode.Light;
    [SerializeField] bool hideWhileIdle = true;
    [SerializeField] float viewAngle = 90f;
    [SerializeField] float eyeHeight = 0.5f;

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
    [Tooltip("Altura del punto que la linterna debe iluminar (el pecho del monstruo)")]
    [SerializeField] float lightCheckHeight = 1f;
    [Tooltip("Segundos que se ve la animación de muerte antes de desaparecer")]
    [SerializeField] float dyingTime = 1.5f;

    [Header("Debug")]
    [SerializeField] State state = State.Idle;

    Vector3 homePosition;
    Quaternion homeRotation;
    float lightTimer;
    float banishTimer;
    float dyingTimer;
    Collider[] colliders;

    Vector3 PlayerTarget => player.position + Vector3.up * targetHeight;
    Vector3 LightPoint => transform.position + Vector3.up * lightCheckHeight;
    float DistanceToPlayer => Vector3.Distance(transform.position, PlayerTarget);
    bool IsLit => flashlight.IsIlluminating(LightPoint);

    public bool IsActive => state == State.Chasing || state == State.Stunned;
    public State CurrentState => state;

    void Awake()
    {
        homePosition = transform.position;
        homeRotation = transform.rotation;
        colliders = GetComponentsInChildren<Collider>();
        GetComponent<Rigidbody>().isKinematic = true;

        // Se busca solo: así funciona como prefab en cualquier parte
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (flashlight == null && player != null)
            flashlight = player.GetComponent<Flashlight>();
    }

    void Start()
    {
        GameManager.Instance.OnPlayerRespawned.AddListener(ResetEnemy);
        ResetEnemy();
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

    // ---------- Activación ----------

    bool ShouldActivate()
    {
        if (DistanceToPlayer > detectionRadius) return false;

        switch (activation)
        {
            case ActivationMode.Light: return flashlight.IsOn;
            case ActivationMode.Sight: return CanSeePlayer();
            default:                   return true;
        }
    }

    bool CanSeePlayer()
    {
        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 toPlayer = PlayerTarget - eye;

        if (Vector3.Angle(transform.forward, toPlayer) > viewAngle * 0.5f)
            return false;

        if (Physics.Linecast(eye, PlayerTarget, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore))
            return hit.collider.CompareTag("Player");

        return true;
    }

    // ---------- Estados ----------

    void UpdateIdle()
    {
        if (ShouldActivate())
        {
            SetVisible(true);
            SetState(State.Chasing);
        }
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
        // Persigue los pies del niño (no un punto en el aire)
        MoveTowards(player.position, chaseSpeed);
    }

    void UpdateStunned()
    {
        if (!IsLit)
        {
            SetState(State.Chasing);
            return;
        }

        lightTimer += Time.deltaTime;

        // Retrocede alejándose del niño...
        Vector3 away = transform.position - player.position;
        away.y = 0f;
        transform.position += away.normalized * retreatSpeed * Time.deltaTime;

        // ...pero sin dejar de mirarlo
        Vector3 toPlayer = -away;
        if (toPlayer.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(toPlayer);

        visual.transform.localPosition = Random.insideUnitSphere * shakeAmount;

        if (lightTimer >= timeToBanish)
            Banish();
    }

    void UpdateBanished()
    {
        // Primero se ve la animación de muerte, luego desaparece
        if (dyingTimer > 0f)
        {
            dyingTimer -= Time.deltaTime;
            if (dyingTimer <= 0f)
                visual.SetActive(false);
        }

        banishTimer -= Time.deltaTime;
        if (banishTimer <= 0f)
            ResetEnemy();
    }

    void UpdateReturning()
    {
        if (ShouldActivate())
        {
            SetState(State.Chasing);
            return;
        }

        MoveTowards(homePosition, returnSpeed);

        if (transform.position == homePosition)
            ResetEnemy();
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
        dyingTimer = dyingTime;
        SetColliders(false);          // ya no puede matar mientras muere
        SetState(State.Banished);
    }

    void ResetEnemy()
    {
        transform.SetPositionAndRotation(homePosition, homeRotation);
        lightTimer = 0f;
        dyingTimer = 0f;
        SetVisible(!hideWhileIdle);
        SetState(State.Idle);
    }

    void SetVisible(bool visible)
    {
        visual.SetActive(visible);
        SetColliders(visible);
    }

    void SetColliders(bool enabled)
    {
        foreach (Collider c in colliders)
            c.enabled = enabled;
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

        // Punto que la linterna tiene que iluminar
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * lightCheckHeight, 0.2f);

        if (activation == ActivationMode.Sight)
        {
            Vector3 eye = transform.position + Vector3.up * eyeHeight;
            Vector3 left = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward;
            Vector3 right = Quaternion.Euler(0, viewAngle * 0.5f, 0) * transform.forward;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(eye, eye + left * detectionRadius);
            Gizmos.DrawLine(eye, eye + right * detectionRadius);
        }
    }
}