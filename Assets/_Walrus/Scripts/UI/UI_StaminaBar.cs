using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_StaminaBar : MonoBehaviour
{
    public static UI_StaminaBar Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private CanvasGroup barCanvasGroup;
    [SerializeField] private Image barFill;

    [Header("Colors")]
    [SerializeField] private Color colorNormal = new Color(0.45f, 0.75f, 1f, 1f);
    [SerializeField] private Color colorExhausted = new Color(0.9f, 0.25f, 0.25f, 1f);

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float hideDelay = 0.8f;

    private Coroutine fadeCoroutine;
    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (barCanvasGroup != null)
            barCanvasGroup.alpha = 0f;

        if (barFill != null)
            barFill.color = colorNormal;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // =============================

    public void UpdateStamina(float current, float max, bool exhausted)
    {
        if (barFill == null) return;

        barFill.fillAmount = current / max;

        barFill.color = exhausted ? colorExhausted : colorNormal;
    }

    public void SetExhausted(bool value)
    {
        if (barFill == null) return;

        barFill.color = value ? colorExhausted : colorNormal;
    }

    public void ShowBar()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (barCanvasGroup == null) return;

        if (barCanvasGroup.alpha < 1f)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeTo(1f));
        }
    }

    public void ScheduleHide()
    {
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeTo(0f));

        hideCoroutine = null;
    }

    private IEnumerator FadeTo(float target)
    {
        if (barCanvasGroup == null) yield break;

        float start = barCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            barCanvasGroup.alpha = Mathf.Lerp(start, target, elapsed / fadeDuration);
            yield return null;
        }

        barCanvasGroup.alpha = target;
        fadeCoroutine = null;
    }
}