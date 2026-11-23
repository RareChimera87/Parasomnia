using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum DeathCause { General, Caida, Dientes, Aplastado, Monstruo, Ojos }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] GameOverScreen gameOverScreen;
    [SerializeField] VictoryScreen victoryScreen;
    [SerializeField] CursorLock cursorLock;
    [SerializeField] string menuSceneName = "MainMenu";

    [Header("Sonidos de muerte")]
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioClip generalDeath;
    [SerializeField] AudioClip fallDeath;
    [SerializeField] AudioClip teethDeath;
    [SerializeField] AudioClip crushedDeath;
    [SerializeField] AudioClip monsterDeath;
    [SerializeField] AudioClip eyesDeath;
    [SerializeField] AudioClip victoryClip;

    public UnityEvent OnPlayerDied;
    public UnityEvent OnPlayerRespawned;

    public bool IsGameOver { get; private set; }

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        if (sfxSource != null) sfxSource.ignoreListenerPause = true;
    }

    public void PlayerDied(DeathCause cause = DeathCause.General)
    {
        if (IsGameOver) return;
        EndGame();
        OnPlayerDied?.Invoke();
        PlayDeathSound(cause);
        gameOverScreen.Show();
    }

    void PlayDeathSound(DeathCause cause)
    {
        AudioClip clip = cause switch
        {
            DeathCause.Caida => fallDeath,
            DeathCause.Dientes => teethDeath,
            DeathCause.Aplastado => crushedDeath,
            DeathCause.Monstruo => monsterDeath,
            DeathCause.Ojos => eyesDeath,
            _ => generalDeath
        };

        if (clip == null) clip = generalDeath;
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
    }

    public void PlayerWon()
    {
        if (IsGameOver) return;
        EndGame();
        if (sfxSource != null && victoryClip != null) sfxSource.PlayOneShot(victoryClip);
        victoryScreen.Show();
    }

    public void RestartLevel()
    {
        AudioListener.pause = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        AudioListener.pause = false;
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
        AudioListener.pause = true;
    }
}