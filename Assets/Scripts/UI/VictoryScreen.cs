using System.Collections;
using UnityEngine;

public class VictoryScreen : MonoBehaviour
{
    [SerializeField] CanvasGroup background;
    [SerializeField] CanvasGroup content;
    [SerializeField] float backgroundFade = 2f;
    [SerializeField] float contentDelay = 0.5f;
    [SerializeField] float contentFade = 1.5f;

    void Awake()
    {
        Hide(background);
        Hide(content);
    }

    public void Show()
    {
        StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        background.blocksRaycasts = true;
        yield return Fade(background, backgroundFade);

        yield return new WaitForSecondsRealtime(contentDelay);

        yield return Fade(content, contentFade);
        content.interactable = true;
        content.blocksRaycasts = true;
    }

    IEnumerator Fade(CanvasGroup group, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Clamp01(t / duration);
            yield return null;
        }
        group.alpha = 1f;
    }

    void Hide(CanvasGroup group)
    {
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }
}