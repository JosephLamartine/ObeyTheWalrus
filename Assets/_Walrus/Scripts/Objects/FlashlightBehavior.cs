using UnityEngine;

public class FlashlightBehavior : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Light flashlightLight;

    [Header("Sway & Lag")]
    [SerializeField] private float swayAmount    = 0.04f;
    [SerializeField] private float swaySmoothing = 8f;

    [Header("Bob al caminar")]
    [SerializeField] private float bobFrequency  = 8f;
    [SerializeField] private float bobAmplitudeY = 0.015f;
    [SerializeField] private float bobAmplitudeX = 0.008f;

    [Header("Batería")]
    [SerializeField] private float drainPerSecond = 0.015f;

    private Vector3 initialLocalPos;
    private float   bobTimer = 0f;
    private CharacterController cc;

    private PlayerInputActions inputActions;
    private Vector2 lookInput; // para el sway sin Input.GetAxis

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        
        inputActions.Player.Flashlight.performed += ctx =>
        {
            if (InventoryManager.Instance.hasFlashlight)
                InventoryManager.Instance.ToggleFlashlight();
        };

        // Reutilizamos el Look para el sway, sin depender de Input.GetAxis
        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled  += ctx => lookInput = Vector2.zero;
    }

    private void OnEnable()  => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Start()
    {
        initialLocalPos = transform.localPosition;
        cc = GetComponentInParent<CharacterController>();

        // DEBUG
        InventoryManager.Instance.PickupFlashlight();

        if (flashlightLight != null)
            flashlightLight.enabled = false;

        InventoryManager.Instance.OnFlashlightToggled += SetLight;
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnFlashlightToggled -= SetLight;
    }

    private void Update()
    {
        if (!InventoryManager.Instance.hasFlashlight) return;

        HandleSway();
        HandleBob();
        HandleDrain();
    }

    private void HandleSway()
    {
        Vector3 swayTarget = new Vector3(
            initialLocalPos.x - lookInput.x * swayAmount,
            initialLocalPos.y - lookInput.y * swayAmount,
            initialLocalPos.z
        );

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            swayTarget,
            swaySmoothing * Time.deltaTime
        );
    }

    private void HandleBob()
    {
        bool isMoving = cc != null && cc.velocity.magnitude > 0.1f && cc.isGrounded;

        if (isMoving)
        {
            bobTimer += Time.deltaTime * bobFrequency;

            transform.localPosition = new Vector3(
                initialLocalPos.x + Mathf.Cos(bobTimer)            * bobAmplitudeX,
                initialLocalPos.y + Mathf.Abs(Mathf.Sin(bobTimer)) * bobAmplitudeY,
                initialLocalPos.z
            );
        }
        else
        {
            bobTimer = 0f;
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                initialLocalPos,
                swaySmoothing * Time.deltaTime
            );
        }
    }

    private void HandleDrain()
    {
        if (InventoryManager.Instance.IsFlashlightOn())
            InventoryManager.Instance.DrainBattery(drainPerSecond * Time.deltaTime);
    }

    private void SetLight(bool state)
    {
        if (flashlightLight != null)
            flashlightLight.enabled = state;
    }
}