using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PlayerStamina — sistema de stamina con UI integrada.
///
/// SETUP:
/// 1. Crea un Canvas en tu HUD. Dentro, crea un objeto vacío "StaminaBar" con CanvasGroup.
/// 2. Dentro de StaminaBar: un RawImage o Image de fondo (opcional) y un Image "Fill"
///    anclado a la izquierda, con Image Type = Filled, Fill Method = Horizontal.
///    La barra se consume desde los extremos hacia el centro — usa dos fills si quieres
///    ese efecto, o un fill simple centrado con pivot en 0.5.
/// 3. Agrega este script al mismo GameObject que PlayerMovement (o a un hijo).
/// 4. Asigna las referencias en el Inspector.
///
/// COMUNICACIÓN CON PlayerMovement:
/// - Llama a IsSprintAllowed() antes de activar sprint en PlayerMovement.
/// - O usa el evento OnSprintBlocked para reaccionar.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class PlayerStamina : MonoBehaviour
{
    // =====================================================================
    // STAMINA CONFIG
    // =====================================================================
    [Header("─── Stamina ───────────────────────────────")]
    [Tooltip("Stamina máxima (siempre 100 según diseño)")]
    public float maxStamina = 100f;

    [Tooltip("Cuánta stamina se consume por segundo corriendo")]
    public float drainRate = 18f;

    [Tooltip("Cuánta stamina se recupera por segundo")]
    public float regenRate = 22f;

    [Tooltip("Segundos de espera antes de que empiece a recuperarse (después de dejar de correr)")]
    public float regenDelay = 1.4f;

    [Tooltip("Umbral mínimo de stamina para poder volver a correr después de agotarse totalmente")]
    [Range(0f, 100f)]
    public float exhaustionRecoveryThreshold = 30f;

    // =====================================================================
    // UI CONFIG
    // =====================================================================
    [Header("─── UI ─────────────────────────────────────")]
    [Tooltip("CanvasGroup del panel completo de la barra")]
    public CanvasGroup barCanvasGroup;

    [Tooltip("Image con Fill Method = Horizontal para el fill de la barra")]
    public Image barFill;

    [Tooltip("Color normal de la barra")]
    public Color colorNormal = new Color(0.45f, 0.75f, 1f, 1f);   // azul claro

    [Tooltip("Color cuando el jugador está exhausto y no puede correr")]
    public Color colorExhausted = new Color(0.9f, 0.25f, 0.25f, 1f); // rojo

    [Tooltip("Segundos que tarda el fade-in / fade-out del panel")]
    public float fadeDuration = 0.35f;

    [Tooltip("Segundos que espera con la barra llena antes de ocultarla")]
    public float hideDelay = 0.8f;

    // =====================================================================
    // AUDIO
    // =====================================================================
    [Header("─── Audio ───────────────────────────────────")]
    [Tooltip("Clip de jadeo/cansancio al agotar la stamina")]
    public AudioClip exhaustedClip;

    // =====================================================================
    // ESTADO PÚBLICO (PlayerMovement puede consultarlo)
    // =====================================================================
    public float  CurrentStamina     => currentStamina;
    public bool   IsExhausted        => isExhausted;
    public bool   IsSprintAllowed    => !isExhausted;

    // =====================================================================
    // PRIVADOS
    // =====================================================================
    private float currentStamina;
    private bool  isExhausted        = false;
    private bool  isSprinting        = false;
    private bool  isRegenerating     = false;

    private Coroutine regenDelayCoroutine;
    private Coroutine fadeCoroutine;
    private Coroutine hideCoroutine;

    private PlayerMovement playerMovement;

    // =====================================================================
    // LIFECYCLE
    // =====================================================================
    private void Awake()
    {
        playerMovement  = GetComponent<PlayerMovement>();
        currentStamina  = maxStamina;

        if (barCanvasGroup != null)
            barCanvasGroup.alpha = 0f;

        if (barFill != null)
            barFill.color = colorNormal;
    }

    private void Update()
    {
        // Leer si el jugador está sprintando DESDE PlayerMovement
        // PlayerMovement expone sprintInput como propiedad pública (ver nota abajo)
        bool wantsToSprint = playerMovement.SprintInput && playerMovement.IsMoving;

        // Aplicar bloqueo por exhaustión
        bool actualSprint = wantsToSprint && !isExhausted;

        HandleStamina(actualSprint);
        UpdateUI();
    }

    // =====================================================================
    // LÓGICA DE STAMINA
    // =====================================================================
    private void HandleStamina(bool sprinting)
    {
        bool wasSprintingLastFrame = isSprinting;
        isSprinting = sprinting;

        if (sprinting)
        {
            // Consumir
            isRegenerating = false;

            if (regenDelayCoroutine != null)
            {
                StopCoroutine(regenDelayCoroutine);
                regenDelayCoroutine = null;
            }

            currentStamina -= drainRate * Time.deltaTime;
            currentStamina  = Mathf.Max(currentStamina, 0f);

            // Aseguramos que la barra esté visible
            ShowBar();

            // Agotamiento total
            if (currentStamina <= 0f && !isExhausted)
            {
                TriggerExhaustion();
            }
        }
        else
        {
            // Acaba de dejar de correr → iniciar delay de regen
            if (wasSprintingLastFrame && !isRegenerating)
            {
                if (regenDelayCoroutine != null)
                    StopCoroutine(regenDelayCoroutine);

                regenDelayCoroutine = StartCoroutine(RegenAfterDelay());
            }

            // Regenerar si corresponde
            if (isRegenerating && currentStamina < maxStamina)
            {
                currentStamina += regenRate * Time.deltaTime;
                currentStamina  = Mathf.Min(currentStamina, maxStamina);

                // Levantar bloqueo de exhaustión al superar el umbral
                if (isExhausted && currentStamina >= exhaustionRecoveryThreshold)
                {
                    isExhausted = false;
                    if (barFill != null)
                        barFill.color = colorNormal;
                }

                // Barra llena → ocultar con delay
                if (currentStamina >= maxStamina)
                {
                    isRegenerating = false;
                    ScheduleHide();
                }
            }
        }
    }

    private void TriggerExhaustion()
    {
        isExhausted    = true;
        currentStamina = 0f;

        if (barFill != null)
            barFill.color = colorExhausted;

        // Sonido de jadeo a través del AudioManager singleton
        if (exhaustedClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(exhaustedClip);
    }

    private IEnumerator RegenAfterDelay()
    {
        isRegenerating = false;
        yield return new WaitForSeconds(regenDelay);
        isRegenerating = true;
        regenDelayCoroutine = null;
    }

    // =====================================================================
    // UI
    // =====================================================================
    private void UpdateUI()
    {
        if (barFill == null) return;
        barFill.fillAmount = currentStamina / maxStamina;
    }

    private void ShowBar()
    {
        // Cancelar hide pendiente
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (barCanvasGroup == null) return;

        // Solo hacer fade-in si no está ya visible
        if (barCanvasGroup.alpha < 1f)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeTo(1f));
        }
    }

    private void ScheduleHide()
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

        float start   = barCanvasGroup.alpha;
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