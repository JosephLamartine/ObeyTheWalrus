using System;
using UnityEngine;
using UnityEngine.Video;

public class VideoDevice : MonoBehaviour
{
    [Header("References")]
    public VideoPlayer videoPlayer;
    public Light screenLight;

    [Header("Pause Overlay")]
    [Tooltip("SpriteRenderer que aparecerá suavemente cuando el video se pause")]
    public SpriteRenderer pauseOverlaySprite;

    [Header("Settings")]
    public bool turnOffLightWhenStopped = true;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.6f;   // Tiempo en segundos para aparecer
    [SerializeField] private float fadeOutDuration = 0.4f;  // Tiempo para desaparecer (opcional)

    // Variables internas para el fade
    private Coroutine currentFadeCoroutine;
    private Color originalOverlayColor;

    public void StartVideo()
    {
        if (videoPlayer == null) return;

        videoPlayer.Play();
        ParanoiaManager.Instance.StartParanoia();

        if (screenLight != null)
            screenLight.enabled = true;

        // Ocultar el overlay cuando se reproduce el video
        if (pauseOverlaySprite != null)
            FadeOverlay(0f, 0f); // Aparece inmediatamente transparente

        Debug.Log($"[VideoDevice] Video iniciado en: {gameObject.name}");
    }
    
    public void PauseVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
            ParanoiaManager.Instance.StopParanoia();

            // Mostrar el overlay con fade suave
            if (pauseOverlaySprite != null)
                FadeOverlay(1f, fadeInDuration);
        }
    }
    
    public void StopVideo()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();

        if (screenLight != null && turnOffLightWhenStopped)
            screenLight.enabled = false;

        // Ocultar el overlay cuando se detiene el video
        if (pauseOverlaySprite != null)
            FadeOverlay(0f, fadeOutDuration);

        Debug.Log($"[VideoDevice] Video detenido en: {gameObject.name}");
    }

    /// <summary>
    /// Realiza el fade suave del sprite de pausa
    /// </summary>
    private void FadeOverlay(float targetAlpha, float duration)
    {
        if (pauseOverlaySprite == null) return;

        // Guardamos el color original la primera vez
        if (originalOverlayColor == default)
            originalOverlayColor = pauseOverlaySprite.color;

        // Detenemos cualquier fade anterior
        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        currentFadeCoroutine = StartCoroutine(FadeCoroutine(targetAlpha, duration));
    }

    private System.Collections.IEnumerator FadeCoroutine(float targetAlpha, float duration)
    {
        if (pauseOverlaySprite == null) yield break;

        Color startColor = pauseOverlaySprite.color;
        Color targetColor = new Color(originalOverlayColor.r, originalOverlayColor.g, originalOverlayColor.b, targetAlpha);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            pauseOverlaySprite.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        // Aseguramos el valor final
        pauseOverlaySprite.color = targetColor;
        currentFadeCoroutine = null;
    }

    // Propiedades existentes (sin cambios)
    public bool IsPlaying => videoPlayer != null && videoPlayer.isPlaying;
    public bool IsPaused  => videoPlayer != null && videoPlayer.isPaused;
    public bool IsPrepared => videoPlayer != null && videoPlayer.isPrepared;
}