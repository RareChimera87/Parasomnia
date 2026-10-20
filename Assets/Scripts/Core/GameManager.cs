using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] int maxLives = 3;
    [SerializeField] Transform spawnPoint;
    [SerializeField] PlayerMovement player;

    public UnityEvent<int> OnLivesChanged;
    public UnityEvent OnGameOver;

    public int Lives { get; private set; }
    Vector3 checkpointPosition;
    float lastDeathTime = -999f;

    void Awake()
    {
        Instance = this;
        Lives = maxLives;
        checkpointPosition = spawnPoint.position;
    }

    void Start()
    {
        OnLivesChanged?.Invoke(Lives);
    }

    public void SetCheckpoint(Vector3 position)
    {
        checkpointPosition = position;
    }

    public void PlayerDied()
    {
        if (Time.time - lastDeathTime < 0.5f) return;
        lastDeathTime = Time.time;
        Lives--;
        Debug.Log($"Vidas restantes: {Lives}");
        OnLivesChanged?.Invoke(Lives);

        if (Lives <= 0)
        {
            GameOver();
            return;
        }

        player.Teleport(checkpointPosition);
    }

    void GameOver()
    {
        Debug.Log("GAME OVER");
        OnGameOver?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}