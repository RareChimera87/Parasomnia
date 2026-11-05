using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] GameOverScreen gameOverScreen;
    [SerializeField] CursorLock cursorLock;

    public UnityEvent OnPlayerDied;
    public UnityEvent OnPlayerRespawned;

    public bool IsGameOver { get; private set; }

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    public void PlayerDied()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        OnPlayerDied?.Invoke();

        Time.timeScale = 0f;

        if (cursorLock != null) cursorLock.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        gameOverScreen.Show();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}