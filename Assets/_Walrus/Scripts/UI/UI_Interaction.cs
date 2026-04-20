using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UI_Interaction : MonoBehaviour
{
    public static UI_Interaction Instance { get; private set; }

    [Header("UI Prompt")]
    [SerializeField] private CanvasGroup promptCanvasGroup;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Image interactionIcon;

    [Header("Interaction Icons")]
    [SerializeField] private Sprite iconInteract;
    [SerializeField] private Sprite iconGrab;
    [SerializeField] private Sprite iconExamine;

    [Header("Fade Settings")]
    [SerializeField] private float fadeSpeed = 8f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (promptCanvasGroup != null)
            promptCanvasGroup.alpha = 0f;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // =========================

    public void Show(string text, InteractionType type)
    {
        if (promptText != null)
            promptText.text = text;

        if (interactionIcon != null)
            interactionIcon.sprite = GetSpriteForType(type);

        if (promptCanvasGroup != null)
            StartFade(1f);
    }

    public void Hide()
    {
        if (promptCanvasGroup != null)
            StartFade(0f);
    }

    private Sprite GetSpriteForType(InteractionType type) => type switch
    {
        InteractionType.Grab => iconGrab,
        InteractionType.Examine => iconExamine,
        _ => iconInteract
    };

    private void StartFade(float targetAlpha)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCanvas(targetAlpha));
    }

    private IEnumerator FadeCanvas(float targetAlpha)
    {
        float startAlpha = promptCanvasGroup.alpha;
        float elapsed = 0f;

        while (!Mathf.Approximately(promptCanvasGroup.alpha, targetAlpha))
        {
            elapsed += Time.deltaTime;
            promptCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed * fadeSpeed);
            yield return null;
        }

        promptCanvasGroup.alpha = targetAlpha;
    }
}