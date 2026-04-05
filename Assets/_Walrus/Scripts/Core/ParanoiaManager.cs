using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.Rendering;
using System.Collections;
using UnityEngine.Rendering.Universal;

/// <summary>
/// OBEDECE A LA MORSA — ParanoiaManager v3
///
/// Un único paranoiaLevel (0–100) gobierna todo.
/// En el Inspector defines en qué % del slider arranca cada efecto.
/// La reversa (post-video) corre el mismo sistema de vuelta a 0.
/// </summary>
public class ParanoiaManager : MonoBehaviour
{
    public static ParanoiaManager Instance { get; private set; }

    // ═══════════════════════════════════════════════════════
    // PARANOIA LEVEL  (el slider maestro)
    // ═══════════════════════════════════════════════════════
    [Header("─── Paranoia Level ───────────────────────")]
    [Range(0f, 100f)]
    public float paranoiaLevel = 0f;          // 0 = calmado, 100 = máximo horror

    [Tooltip("Velocidad a la que sube el nivel mientras el video corre (unidades/seg)")]
    public float riseSpeed    = 14f;

    [Tooltip("Velocidad a la que baja al hacer StopParanoia o en la reversa")]
    public float fallSpeed    = 22f;

    [Tooltip("Multiplicador de velocidad extra durante la reversa del teletransporte (ej: 3 = 3x más rápido)")]
    public float reverseSpeedMultiplier = 3f;

    // ═══════════════════════════════════════════════════════
    // FASES — % del slider en que cada efecto comienza
    // ═══════════════════════════════════════════════════════
    [Header("─── Umbrales de Fase (0–100) ─────────────")]
    [Tooltip("% a partir del cual empieza el Hue Shift")]
    public float hueShiftStart         =  0f;

    [Tooltip("% a partir del cual empieza la Viñeta")]
    public float vignetteStart         = 10f;

    [Tooltip("% a partir del cual empieza Chromatic Aberration")]
    public float chromaticStart        = 20f;

    [Tooltip("% a partir del cual empieza Lens Distortion")]
    public float lensDistortionStart   = 35f;

    [Tooltip("% a partir del cual empieza Panini Projection")]
    public float paniniStart           = 50f;

    [Tooltip("% a partir del cual el video EMPIEZA A REPRODUCIRSE (antes de ser visible)")]
    public float videoPlayStart        = 45f;

    [Tooltip("% a partir del cual empieza el fade-in del video de sangre")]
    public float videoFadeStart        = 55f;

    [Tooltip("% en que el video llega garantizado a 100% de alpha (debe ser < teleportThreshold)")]
    public float videoFullAlpha        = 78f;

    [Tooltip("% a partir del cual se dispara el teletransporte")]
    public float teleportThreshold     = 85f;

    // ═══════════════════════════════════════════════════════
    // INTENSIDADES MÁXIMAS
    // ═══════════════════════════════════════════════════════
    [Header("─── Intensidades Máximas ──────────────────")]
    public float maxHueShiftSpeed      = 16f;   // frecuencia máxima de la oscilación
    public float maxHueShiftAmount     = 180f;  // grados máximos de hue shift

    [Range(0f, 1f)]
    public float maxVignetteIntensity  = 0.55f;

    [Range(0f, 1f)]
    public float maxChromaticIntensity = 1f;

    [Range(-1f, 1f)]
    public float maxLensDistortion     = -0.45f;  // negativo = barrel

    [Range(0f, 1f)]
    public float maxPaniniDistance     = 0.6f;

    // ═══════════════════════════════════════════════════════
    // POST-PROCESS VOLUME
    // ═══════════════════════════════════════════════════════
    [Header("─── Post Process ─────────────────────────")]
    public Volume postProcessVolume;

    // ═══════════════════════════════════════════════════════
    // VIDEO & UI
    // ═══════════════════════════════════════════════════════
    [Header("─── Video & UI ───────────────────────────")]
    public VideoPlayer paranoiaVideo;
    public RawImage    bloodRawImage;

    // ═══════════════════════════════════════════════════════
    // ESCENA DESTINO
    // ═══════════════════════════════════════════════════════
    [Header("─── Escena Destino ──────────────────────")]
    public string dimensionSceneName = "OtraDimension";

    // ───────────────────────────────────────────────────────
    // PRIVADOS
    // ───────────────────────────────────────────────────────
    private ColorAdjustments    colorAdj;
    private Vignette            vignette;
    private ChromaticAberration chromatic;
    private LensDistortion      lensDistortion;
    private PaniniProjection    panini;

    // valores originales del perfil (para restaurar)
    private float origHue, origVignette, origChromatic, origLensDistortion, origPanini;

    private bool  isRising;
    private bool  hasTeleported;
    private bool  videoStarted;   // video.Play() fue llamado
    private bool  videoFading;    // el fade-in está activo
    private float huePhase;   // fase acumulada — evita el salto que causaba Time.time

    private Coroutine sequenceCoroutine;
    private Coroutine reverseCoroutine;

    // ═══════════════════════════════════════════════════════
    // LIFECYCLE
    // ═══════════════════════════════════════════════════════

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BindPostProcess();
        ResetAllEffects();
        SetBloodAlpha(0f);
    }

    private void Update()
    {
        if (isRising)
            paranoiaLevel = Mathf.Min(paranoiaLevel + riseSpeed * Time.deltaTime, 100f);

        ApplyEffects(paranoiaLevel);
    }

    // ═══════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════

    /// <summary>Inicia la secuencia de paranoia completa.</summary>
    public void StartParanoia()
    {
        if (isRising) return;

        hasTeleported = false;
        videoStarted  = false;
        isRising      = true;

        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        sequenceCoroutine = StartCoroutine(SequenceWatcher());
    }

    /// <summary>Interrumpe y baja todo suavemente.</summary>
    public void StopParanoia()
    {
        isRising = false;

        if (sequenceCoroutine != null) { StopCoroutine(sequenceCoroutine); sequenceCoroutine = null; }
        if (reverseCoroutine  != null) { StopCoroutine(reverseCoroutine);  reverseCoroutine  = null; }

        if (paranoiaVideo != null) paranoiaVideo.Stop();

        reverseCoroutine = StartCoroutine(FallToZero());
    }

    /// <summary>Reset inmediato sin animación.</summary>
    public void ResetParanoia()
    {
        StopAllCoroutines();
        isRising      = false;
        hasTeleported = false;
        videoStarted  = false;
        paranoiaLevel = 0f;

        if (paranoiaVideo != null) paranoiaVideo.Stop();
        ResetAllEffects();
        SetBloodAlpha(0f);
        huePhase = 0f;
    }

    // ═══════════════════════════════════════════════════════
    // WATCHER — dispara eventos en umbrales
    // ═══════════════════════════════════════════════════════

    private IEnumerator SequenceWatcher()
    {
        while (true)
        {
            // Arrancar PLAYBACK del video (sin ser visible aún)
            if (!videoStarted && paranoiaLevel >= videoPlayStart)
            {
                videoStarted = true;
                if (paranoiaVideo != null) { paranoiaVideo.Stop(); paranoiaVideo.Play(); }
            }

            // Teleporte
            if (!hasTeleported && paranoiaLevel >= teleportThreshold)
            {
                hasTeleported = true;
                isRising      = false;
                yield return StartCoroutine(TeleportSequence());
                yield break;
            }

            yield return null;
        }
    }

    // ═══════════════════════════════════════════════════════
    // APPLY EFFECTS — llamado cada frame desde Update
    // ═══════════════════════════════════════════════════════

    private void ApplyEffects(float level)
    {
        // ── Hue Shift ──────────────────────────────────────
        if (colorAdj != null)
        {
            float t = NormalizedPhase(level, hueShiftStart);
            if (t > 0f)
            {
                float speed = Mathf.Lerp(0.3f, maxHueShiftSpeed, t * t);
                huePhase += speed * Time.deltaTime;
                colorAdj.hueShift.value = Mathf.Sin(huePhase) * maxHueShiftAmount * t;
            }
            else
            {
                colorAdj.hueShift.value = origHue;
            }
        }

        // ── Vignette ───────────────────────────────────────
        if (vignette != null)
        {
            float t = NormalizedPhase(level, vignetteStart);
            vignette.intensity.value = Mathf.Lerp(origVignette, maxVignetteIntensity, t);
        }

        // ── Chromatic Aberration ───────────────────────────
        if (chromatic != null)
        {
            float t = NormalizedPhase(level, chromaticStart);
            chromatic.intensity.value = Mathf.Lerp(origChromatic, maxChromaticIntensity, t);
        }

        // ── Lens Distortion ────────────────────────────────
        if (lensDistortion != null)
        {
            float t = NormalizedPhase(level, lensDistortionStart);
            lensDistortion.intensity.value = Mathf.Lerp(origLensDistortion, maxLensDistortion, t);
        }

        // ── Panini Projection ──────────────────────────────
        if (panini != null)
        {
            float t = NormalizedPhase(level, paniniStart);
            panini.distance.value = Mathf.Lerp(origPanini, maxPaniniDistance, t);
        }

        // ── Blood image alpha ──────────────────────────────
        // Mapea: videoFadeStart → 0 alpha, videoFullAlpha → 1 alpha (garantizado)
        if (bloodRawImage != null)
        {
            if (level <= videoFadeStart)
                SetBloodAlpha(0f);
            else if (level >= videoFullAlpha)
                SetBloodAlpha(1f);
            else
            {
                float t = Mathf.Clamp01((level - videoFadeStart) / (videoFullAlpha - videoFadeStart));
                SetBloodAlpha(Mathf.SmoothStep(0f, 1f, t));
            }
        }
    }

    // ═══════════════════════════════════════════════════════
    // TELEPORT SEQUENCE
    // ═══════════════════════════════════════════════════════

    private IEnumerator TeleportSequence()
    {
        // Garantiza alpha 1 antes de cualquier cosa
        SetBloodAlpha(1f);
        yield return new WaitForSeconds(1.0f);

        bool sceneExists = SceneExistsInBuild(dimensionSceneName);

        if (sceneExists)
        {
            SceneManager.LoadScene(dimensionSceneName, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning($"[ParanoiaManager] No scene found: '{dimensionSceneName}' — performing reverse reveal.");
            yield return StartCoroutine(ReverseReveal());
        }
    }

    /// <summary>
    /// Reversa acelerada: baja paranoiaLevel rápido, efectos se normalizan solos.
    /// El video hace fade-out y se pausa cuando ya no es visible.
    /// </summary>
    private IEnumerator ReverseReveal()
    {
        float fastFall = fallSpeed * reverseSpeedMultiplier;
        bool  videoPaused = false;

        while (paranoiaLevel > 0f)
        {
            paranoiaLevel = Mathf.Max(paranoiaLevel - fastFall * Time.deltaTime, 0f);

            // Pausa el video cuando el alpha ya bajó suficiente (invisible)
            if (!videoPaused && paranoiaVideo != null && paranoiaLevel < videoFadeStart)
            {
                videoPaused = true;
                paranoiaVideo.Pause();
            }

            yield return null;
        }

        paranoiaLevel = 0f;
        huePhase      = 0f;
        ResetAllEffects();
        SetBloodAlpha(0f);
        if (paranoiaVideo != null) paranoiaVideo.Stop();

        Debug.Log("[ParanoiaManager] Reverse reveal complete — nueva realidad revelada.");
    }

    // ═══════════════════════════════════════════════════════
    // FALL TO ZERO — baja el nivel, efectos se normalizan solos via ApplyEffects
    // ═══════════════════════════════════════════════════════

    // FallToZero — usado por StopParanoia() (velocidad normal, sin multiplicador)
    private IEnumerator FallToZero()
    {
        bool videoPaused = false;
        while (paranoiaLevel > 0f)
        {
            paranoiaLevel = Mathf.Max(paranoiaLevel - fallSpeed * Time.deltaTime, 0f);

            if (!videoPaused && paranoiaVideo != null && paranoiaLevel < videoFadeStart)
            {
                videoPaused = true;
                paranoiaVideo.Pause();
            }

            yield return null;
        }

        paranoiaLevel = 0f;
        huePhase      = 0f;
        ResetAllEffects();
        SetBloodAlpha(0f);
        if (paranoiaVideo != null) paranoiaVideo.Stop();
    }

    // ═══════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Devuelve 0–1: qué tan avanzado está 'level' dentro del rango [startPct, 100].
    /// </summary>
    private float NormalizedPhase(float level, float startPct)
    {
        if (level <= startPct) return 0f;
        return Mathf.Clamp01((level - startPct) / (100f - startPct));
    }

    private IEnumerator FadeBloodTo(float target, float duration)
    {
        if (bloodRawImage == null) yield break;
        float start   = bloodRawImage.color.a;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetBloodAlpha(Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, elapsed / duration)));
            yield return null;
        }
        SetBloodAlpha(target);
    }

    private void SetBloodAlpha(float a)
    {
        if (bloodRawImage != null)
            bloodRawImage.color = new Color(1f, 1f, 1f, a);
    }

    private void BindPostProcess()
    {
        if (postProcessVolume == null) return;
        var p = postProcessVolume.profile;

        if (p.TryGet(out colorAdj))        origHue            = colorAdj.hueShift.value;
        if (p.TryGet(out vignette))         origVignette       = vignette.intensity.value;
        if (p.TryGet(out chromatic))        origChromatic      = chromatic.intensity.value;
        if (p.TryGet(out lensDistortion))   origLensDistortion = lensDistortion.intensity.value;
        if (p.TryGet(out panini))           origPanini         = panini.distance.value;
    }

    private void ResetAllEffects()
    {
        if (colorAdj       != null) colorAdj.hueShift.value         = origHue;
        if (vignette       != null) vignette.intensity.value         = origVignette;
        if (chromatic      != null) chromatic.intensity.value        = origChromatic;
        if (lensDistortion != null) lensDistortion.intensity.value   = origLensDistortion;
        if (panini         != null) panini.distance.value            = origPanini;
    }

    private static bool SceneExistsInBuild(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneManager.GetSceneByBuildIndex(i).path;
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
    }
}