using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform cameraTransform;

    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float gravity = -19.62f;

    [Header("Mirar")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float gamepadSensitivity = 180f;
    [SerializeField] private float verticalLookRange = 80f;

    [Header("Camera Bob")]
    [SerializeField] private float walkBobFrequency = 8f;
    [SerializeField] private float walkBobAmplitudeY = 0.05f;
    [SerializeField] private float walkBobAmplitudeX = 0.025f;
    [SerializeField] private float sprintBobFrequency = 14f;
    [SerializeField] private float sprintBobAmplitudeY = 0.09f;
    [SerializeField] private float sprintBobAmplitudeX = 0.045f;
    [SerializeField] private float bobSmoothing = 10f;

    [Header("Pasos")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] walkStepClips;
    [SerializeField] private AudioClip[] sprintStepClips;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float sprintStepInterval = 0.32f;
    [Range(0f, 1f)] [SerializeField] private float footstepVolume = 0.6f;

    private Vector3 playerVelocity;
    private bool isGrounded;
    private float verticalLookRotation = 0f;

    private Vector3 cameraInitialLocalPos;
    private float bobTimer = 0f;

    private float stepTimer = 0f;
    private int lastStepIndex = -1;

    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool sprintInput;
    private bool usingStick = false;

    private bool isAndroidPlatform;
    public bool lockCursor;

    public bool SprintInput => sprintInput;
    public bool IsMoving => moveInput.sqrMagnitude > 0.01f && isGrounded;

    private PlayerStamina staminaSystem;

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        staminaSystem = GetComponent<PlayerStamina>();

        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Sprint.performed += ctx => sprintInput = true;
        inputActions.Player.Sprint.canceled += ctx => sprintInput = false;

        inputActions.Player.Look.performed += OnLookPerformed;
        inputActions.Player.Look.canceled += OnLookCanceled;

        isAndroidPlatform = Application.platform == RuntimePlatform.Android;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Start()
    {
        cameraInitialLocalPos = cameraTransform.localPosition;
    
        if (!isAndroidPlatform && lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        UI_Master.Instance.ShowInventoryHUD();
    }

    private void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && playerVelocity.y < 0)
            playerVelocity.y = -2f;

        // MOVIMIENTO
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

        bool canSprint = staminaSystem == null || staminaSystem.IsSprintAllowed;
        float currentSpeed = (sprintInput && canSprint) ? sprintSpeed : walkSpeed;

        controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);

        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        // MIRAR
        HandleLook();

        // BOB + PASOS
        bool isMoving = moveInput.sqrMagnitude > 0.01f && isGrounded;

        if (isMoving)
        {
            HandleCameraBob();
            HandleFootsteps();
        }
        else
        {
            ResetCameraBob();
            stepTimer = 0f;
        }
    }

    private void HandleLook()
    {
        float lookX, lookY;

        if (usingStick)
        {
            lookX = lookInput.x * gamepadSensitivity * Time.deltaTime;
            lookY = lookInput.y * gamepadSensitivity * Time.deltaTime;
        }
        else
        {
            lookX = lookInput.x * mouseSensitivity;
            lookY = lookInput.y * mouseSensitivity;
        }

        verticalLookRotation -= lookY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -verticalLookRange, verticalLookRange);

        cameraTransform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
        transform.Rotate(Vector3.up * lookX);
    }

    private void OnLookPerformed(InputAction.CallbackContext ctx)
    {
        string path = ctx.control.path.ToLowerInvariant();

        if (isAndroidPlatform && (path.Contains("pointer") || path.Contains("mouse")))
            return;

        lookInput = ctx.ReadValue<Vector2>();
        usingStick = path.Contains("stick");
    }

    private void OnLookCanceled(InputAction.CallbackContext ctx)
    {
        string path = ctx.control.path.ToLowerInvariant();

        if (isAndroidPlatform && (path.Contains("pointer") || path.Contains("mouse")))
            return;

        lookInput = Vector2.zero;
        usingStick = false;
    }

    private void HandleCameraBob()
    {
        float freq = sprintInput ? sprintBobFrequency : walkBobFrequency;
        float ampY = sprintInput ? sprintBobAmplitudeY : walkBobAmplitudeY;
        float ampX = sprintInput ? sprintBobAmplitudeX : walkBobAmplitudeX;

        bobTimer += Time.deltaTime * freq;

        Vector3 bobOffset = new Vector3(
            Mathf.Cos(bobTimer * 0.5f) * ampX,
            Mathf.Abs(Mathf.Sin(bobTimer)) * ampY,
            0f
        );

        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            cameraInitialLocalPos + bobOffset,
            bobSmoothing * Time.deltaTime
        );
    }

    private void ResetCameraBob()
    {
        bobTimer = 0f;
        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            cameraInitialLocalPos,
            bobSmoothing * Time.deltaTime
        );
    }

    private void HandleFootsteps()
    {
        float interval = sprintInput ? sprintStepInterval : walkStepInterval;
        stepTimer += Time.deltaTime;

        if (stepTimer >= interval)
        {
            stepTimer = 0f;
            PlayFootstep();
        }
    }

    private void PlayFootstep()
    {
        if (footstepSource == null) return;

        AudioClip[] clips = sprintInput ? sprintStepClips : walkStepClips;
        if (clips.Length == 0) return;

        int index;
        do { index = Random.Range(0, clips.Length); }
        while (clips.Length > 1 && index == lastStepIndex);

        lastStepIndex = index;
        footstepSource.PlayOneShot(clips[index], footstepVolume);
    }

    private void OnDrawGizmosSelected()
    {
        if (controller == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + controller.center, controller.radius);
    }
}