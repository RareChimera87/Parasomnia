using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    [SerializeField] ShadowEnemy enemy;
    [SerializeField] Animator animator;
    [SerializeField] float crossFade = 0.2f;
    [Tooltip("Si el niño muere a esta distancia o menos, hace la animación de ataque")]
    [SerializeField] float attackDistance = 2.5f;

    ShadowEnemy.State playedState;
    bool wasActive;
    bool attacking;
    Transform player;

    void Start()
    {
        if (enemy == null) enemy = GetComponent<ShadowEnemy>();
        if (animator == null) animator = GetComponentInChildren<Animator>(true);

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;

        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerDied.AddListener(OnPlayerDied);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerDied.RemoveListener(OnPlayerDied);
    }

    void Update()
    {
        if (attacking) return;

        // El modelo se oculta y se muestra: cuando reaparece o cambia de estado, pone la animación correcta
        bool active = animator.isActiveAndEnabled;
        if (active && (!wasActive || enemy.CurrentState != playedState))
        {
            playedState = enemy.CurrentState;
            Play(playedState);
        }
        wasActive = active;
    }

    void Play(ShadowEnemy.State s)
    {
        string anim = s switch
        {
            ShadowEnemy.State.Chasing   => "Run",
            ShadowEnemy.State.Stunned   => "Stunned",
            ShadowEnemy.State.Banished  => "Dying",
            ShadowEnemy.State.Returning => "Walk",
            _                           => "Idle"
        };
        animator.CrossFadeInFixedTime(anim, crossFade);
    }

    void OnPlayerDied()
    {
        if (player == null || !animator.isActiveAndEnabled) return;
        if (Vector3.Distance(transform.position, player.position) > attackDistance) return;

        attacking = true;
        // El juego se pausa al morir: que la animación corra igual
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        animator.CrossFadeInFixedTime("Attack", 0.1f);
    }
}