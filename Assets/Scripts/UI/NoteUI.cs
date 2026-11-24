using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class NoteUI : MonoBehaviour
{
    public static NoteUI Instance { get; private set; }

    [SerializeField] CanvasGroup group;
    [SerializeField] Image noteImage;
    [SerializeField] AudioSource audioSource;
    [Tooltip("Suena al abrir la nota (papel 2)")]
    [SerializeField] AudioClip openClip;
    [Tooltip("Opcional: suena al cerrar")]
    [SerializeField] AudioClip closeClip;
    [SerializeField] float fadeTime = 0.25f;

    public bool IsOpen { get; private set; }

    Action onClosed;
    float targetAlpha;
    float openTime;

    void Awake()
    {
        Instance = this;
        group.alpha = 0f;
        targetAlpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    public void Show(Sprite sprite, Action closed = null)
    {
        if (IsOpen) return;

        noteImage.sprite = sprite;
        noteImage.preserveAspect = true;
        onClosed = closed;

        IsOpen = true;
        targetAlpha = 1f;
        openTime = Time.unscaledTime;
        group.blocksRaycasts = true;

        Time.timeScale = 0f;   // pausa el juego mientras se lee
        Play(openClip);
    }

    void Update()
    {
        // Aparecer / desaparecer suave (funciona con el juego pausado)
        group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, Time.unscaledDeltaTime / fadeTime);

        if (!IsOpen) return;
        if (Time.unscaledTime - openTime < 0.3f) return;   // evita cerrarla sin querer al abrirla

        bool close =
            (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame ||
                                          Keyboard.current.enterKey.wasPressedThisFrame)) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (close) Close();
    }

    void Close()
    {
        IsOpen = false;
        targetAlpha = 0f;
        group.blocksRaycasts = false;

        Time.timeScale = 1f;
        Play(closeClip);

        Action callback = onClosed;
        onClosed = null;
        callback?.Invoke();
    }

    void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }
}