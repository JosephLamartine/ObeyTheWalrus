using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerStamina : MonoBehaviour
{
    [Header("─── Stamina ───────────────────────────────")]
    public float maxStamina = 100f;
    public float drainRate = 18f;
    public float regenRate = 22f;
    public float regenDelay = 1.4f;

    [Range(0f, 100f)]
    public float exhaustionRecoveryThreshold = 30f;

    [Header("─── Audio ───────────────────────────────────")]
    public AudioClip exhaustedClip;

    public float CurrentStamina  => currentStamina;
    public bool  IsExhausted     => isExhausted;
    public bool  IsSprintAllowed => !isExhausted;

    private float currentStamina;
    private bool  isExhausted    = false;
    private bool  isSprinting    = false;
    private bool  isRegenerating = false;

    private Coroutine regenDelayCoroutine;

    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        currentStamina = maxStamina;
    }

    private void Update()
    {
        bool wantsToSprint = playerMovement.SprintInput && playerMovement.IsMoving;
        bool actualSprint  = wantsToSprint && !isExhausted;

        HandleStamina(actualSprint);

        // 🔥 Comunicación con UI (sin referencia directa)
        UI_StaminaBar.Instance?.UpdateStamina(currentStamina, maxStamina, isExhausted);

        if (actualSprint)
            UI_StaminaBar.Instance?.ShowBar();
    }

    private void HandleStamina(bool sprinting)
    {
        bool wasSprintingLastFrame = isSprinting;
        isSprinting = sprinting;

        if (sprinting)
        {
            isRegenerating = false;

            if (regenDelayCoroutine != null)
            {
                StopCoroutine(regenDelayCoroutine);
                regenDelayCoroutine = null;
            }

            currentStamina -= drainRate * Time.deltaTime;
            currentStamina  = Mathf.Max(currentStamina, 0f);

            if (currentStamina <= 0f && !isExhausted)
            {
                TriggerExhaustion();
            }
        }
        else
        {
            if (wasSprintingLastFrame && !isRegenerating)
            {
                if (regenDelayCoroutine != null)
                    StopCoroutine(regenDelayCoroutine);

                regenDelayCoroutine = StartCoroutine(RegenAfterDelay());
            }

            if (isRegenerating && currentStamina < maxStamina)
            {
                currentStamina += regenRate * Time.deltaTime;
                currentStamina  = Mathf.Min(currentStamina, maxStamina);

                if (isExhausted && currentStamina >= exhaustionRecoveryThreshold)
                {
                    isExhausted = false;
                }

                if (currentStamina >= maxStamina)
                {
                    isRegenerating = false;
                    UI_StaminaBar.Instance?.ScheduleHide();
                }
            }
        }
    }

    private void TriggerExhaustion()
    {
        isExhausted    = true;
        currentStamina = 0f;

        UI_StaminaBar.Instance?.SetExhausted(true);

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
}