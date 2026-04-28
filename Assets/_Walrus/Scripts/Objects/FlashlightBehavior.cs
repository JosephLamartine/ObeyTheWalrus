using UnityEngine;
using System.Collections;

public class FlashlightBehavior : MonoBehaviour
{
    [Header("Light")]
    [SerializeField] private Light flashlightLight;

    [Header("Sway & Lag")]
    [SerializeField] private float swayAmount    = 0.04f;
    [SerializeField] private float swaySmoothing = 8f;

    [Header("Walking Bob")]
    [SerializeField] private float bobFrequency  = 8f;
    [SerializeField] private float bobAmplitudeY = 0.015f;
    [SerializeField] private float bobAmplitudeX = 0.008f;

    [Header("Raise / Lower")]
    [SerializeField] private Vector3 hiddenOffset  = new Vector3(0f, -0.3f, 0f);
    [SerializeField] private float   raiseDuration = 0.3f;  // segundos en subir
    [SerializeField] private float   lowerDuration = 0.3f;  // segundos en bajar
    [SerializeField] private float   hideDelay     = 2f;    // delay antes de bajar al apagar

    [Header("Battery Settings")]
    [SerializeField] private float drainPerSecond = 0.015f;
    
    [Header("Sounds")]
    [SerializeField] private AudioClip sndFlashlightOn;
    [SerializeField] private AudioClip sndFlashlightOff;


    private Vector3 initialLocalPos;
    private Vector3 hiddenLocalPos;

    private bool  isVisible = false; // true = arriba (con o sin luz)
    private float bobTimer  = 0f;
    private CharacterController cc;
    private PlayerInputActions inputActions;
    private Vector2 lookInput;
    private Coroutine moveCoroutine;

    private void Awake()
    {
        inputActions = new PlayerInputActions();

        inputActions.Player.Flashlight.performed += ctx =>
        {
            if (InventoryManager.Instance.hasFlashlight)
                InventoryManager.Instance.ToggleFlashlight();
        };

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled  += ctx => lookInput = Vector2.zero;
    }

    private void OnEnable()  => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Start()
    {
        initialLocalPos = transform.localPosition;
        hiddenLocalPos  = initialLocalPos + hiddenOffset;

        transform.localPosition = hiddenLocalPos;

        cc = GetComponentInParent<CharacterController>();

        InventoryManager.Instance.PickupFlashlight(); // DEBUG

        if (flashlightLight != null)
            flashlightLight.enabled = false;

        InventoryManager.Instance.OnFlashlightToggled += OnToggle;
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnFlashlightToggled -= OnToggle;
    }

    private void Update()
    {
        if (!InventoryManager.Instance.hasFlashlight) return;
        if (isVisible) // sway y bob solo cuando está arriba
        {
            HandleSway();
            HandleBob();
        }
        HandleDrain();
    }

    // ── Toggle ────────────────────────────────────────────────
    private void OnToggle(bool state)
    {
        if (state)
        {
            // Prender: sube → luego enciende la luz al llegar arriba
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(RaiseAndLight());
        }
        else
        {
            // Apagar: apaga luz → espera delay → baja
            if (flashlightLight != null) flashlightLight.enabled = false;
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(WaitAndLower());
        }
    }

    private IEnumerator RaiseAndLight()
    {
        isVisible = true;
        yield return StartCoroutine(MoveTo(initialLocalPos, raiseDuration));
        AudioManager.Instance.PlaySFX(sndFlashlightOn);
        if (flashlightLight != null) flashlightLight.enabled = true;
    }

    private IEnumerator WaitAndLower()
    {
        // Espera con sway activo todavía
        AudioManager.Instance.PlaySFX(sndFlashlightOff);
        yield return new WaitForSeconds(hideDelay);
        isVisible = false;
        yield return StartCoroutine(MoveTo(hiddenLocalPos, lowerDuration));
    }

    // Mueve suavemente de donde está al target en 'duration' segundos
    private IEnumerator MoveTo(Vector3 target, float duration)
    {
        Vector3 start   = transform.localPosition;
        float   elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localPosition = target;
    }

    // ── Sway ──────────────────────────────────────────────────
    private void HandleSway()
    {
        Vector3 current    = transform.localPosition;
        Vector3 swayTarget = new Vector3(
            initialLocalPos.x - lookInput.x * swayAmount,
            current.y,
            initialLocalPos.z
        );

        transform.localPosition = Vector3.Lerp(current, swayTarget, swaySmoothing * Time.deltaTime);
    }

    // ── Bob ───────────────────────────────────────────────────
    private void HandleBob()
    {
        bool isMoving = cc != null && cc.velocity.magnitude > 0.1f && cc.isGrounded;

        if (isMoving)
        {
            bobTimer += Time.deltaTime * bobFrequency;
            Vector3 current = transform.localPosition;
            transform.localPosition = new Vector3(
                initialLocalPos.x + Mathf.Cos(bobTimer) * bobAmplitudeX,
                current.y,
                initialLocalPos.z
            );
        }
        else
        {
            bobTimer = 0f;
        }
    }

    // ── Drain ─────────────────────────────────────────────────
    private void HandleDrain()
    {
        if (InventoryManager.Instance.flashlightOn)
            InventoryManager.Instance.DrainBattery(drainPerSecond);
    }
}