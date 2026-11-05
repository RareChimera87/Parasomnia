using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] GameOverScreen gameOverScreen;
    [SerializeField] VictoryScreen victoryScreen;
    [SerializeField] CursorLock cursorLock;
    [SerializeField] string menuSceneName = "MainMenu";

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
        EndGame();
        OnPlayerDied?.Invoke();
        gameOverScreen.Show();
    }

    public void PlayerWon()
    {
        if (IsGameOver) return;
        EndGame();
        victoryScreen.Show();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void EndGame()
    {
        IsGameOver = true;
        Time.timeScale = 0f;

        if (cursorLock != null) cursorLock.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}