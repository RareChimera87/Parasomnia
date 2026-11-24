using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DeathArtScreen : MonoBehaviour
{
    [Serializable]
    public class DeathArt
    {
        public DeathCause cause;
        [Tooltip("Si pones varias, sale una al azar")]
        public Sprite[] images;
    }

    [SerializeField] CanvasGroup group;
    [SerializeField] Image image;
    [SerializeField] DeathArt[] arts;
    [SerializeField] float fadeIn = 0.4f;
    [SerializeField] float holdTime = 3f;
    [SerializeField] float fadeOut = 0.5f;

    void Awake()
    {
        group.alpha = 0f;
        group.blocksRaycasts = false;
    }

    // Muestra el arte de esa muerte y después llama a "then" (la pantalla de TÚ MORISTE)
    public void Show(DeathCause cause, Action then)
    {
        Sprite sprite = Pick(cause);
        if (sprite == null)
        {
            then?.Invoke();   // esa muerte no tiene arte: directo a TÚ MORISTE
            return;
        }

        image.sprite = sprite;
        image.preserveAspect = true;
        StartCoroutine(Sequence(then));
    }

    Sprite Pick(DeathCause cause)
    {
        foreach (DeathArt art in arts)
            if (art.cause == cause && art.images != null && art.images.Length > 0)
                return art.images[UnityEngine.Random.Range(0, art.images.Length)];
        return null;
    }

    IEnumerator Sequence(Action then)
    {
        group.blocksRaycasts = true;
        yield return Fade(0f, 1f, fadeIn);

        // Se queda un rato; con clic o E se salta
        float t = 0f;
        while (t < holdTime)
        {
            t += Time.unscaledDeltaTime;
            if (t > 0.5f && SkipPressed()) break;
            yield return null;
        }

        then?.Invoke();
        yield return Fade(1f, 0f, fadeOut);
        group.blocksRaycasts = false;
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
    }

    bool SkipPressed()
    {
        return (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
               (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame);
    }
}
