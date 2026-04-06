using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform cameraTransform;

    [Header("Movimiento")]
    [SerializeField] private float walkSpeed   = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float gravity     = -19.62f;
    [SerializeField] private float jumpHeight  = 2.5f;

    [Header("Mirar")]
    [SerializeField] private float mouseSensitivity   = 2f;
    [SerializeField] private float gamepadSensitivity = 180f; // para stick / virtual joystick
    [SerializeField] private float verticalLookRange  = 80f;

    [Header("Camera Bob")]
    [SerializeField] private float walkBobFrequency    = 8f;
    [SerializeField] private float walkBobAmplitudeY   = 0.05f;
    [SerializeField] private float walkBobAmplitudeX   = 0.025f;
    [SerializeField] private float sprintBobFrequency  = 14f;
    [SerializeField] private float sprintBobAmplitudeY = 0.09f;
    [SerializeField] private float sprintBobAmplitudeX = 0.045f;
    [SerializeField] private float bobSmoothing        = 10f;

    [Header("Pasos")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] walkStepClips;
    [SerializeField] private AudioClip[] sprintStepClips;
    [SerializeField] private float walkStepInterval   = 0.5f;
    [SerializeField] private float sprintStepInterval = 0.32f;
    [Range(0f, 1f)]
    [SerializeField] private float footstepVolume = 0.6f;

    // estado
    private Vector3 playerVelocity;
    private bool    isGrounded;
    private float   verticalLookRotation = 0f;

    // bob
    private Vector3 cameraInitialLocalPos;
    private float   bobTimer = 0f;

    // pasos
    private float stepTimer     = 0f;
    private int   lastStepIndex = -1;

    // input
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool    sprintInput;
    private bool    usingStick = false; // true = gamepad/virtual stick, false = mouse
    public  bool    lockCursor;

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController>();

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed   += ctx => moveInput   = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled    += ctx => moveInput   = Vector2.zero;

        inputActions.Player.Look.performed   += ctx =>
        {
            lookInput  = ctx.ReadValue<Vector2>();
            // Detectar si viene de stick (controlPath contiene "Stick" o "rightStick")
            usingStick = ctx.control.path.Contains("Stick") || ctx.control.path.Contains("stick");
        };
        inputActions.Player.Look.canceled    += ctx =>
        {
            lookInput  = Vector2.zero;
            usingStick = false;
        };

        inputActions.Player.Sprint.performed += ctx => sprintInput = true;
        inputActions.Player.Sprint.canceled  += ctx => sprintInput = false;
    }

    private void OnEnable()  => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Start()
    {
        cameraInitialLocalPos = cameraTransform.localPosition;
        GameManager.Instance.ShowControlHUD();
    }

    private void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && playerVelocity.y < 0)
            playerVelocity.y = -2f;

        // ── MOVIMIENTO ───────────────────────────────────────────────
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        float   currentSpeed  = sprintInput ? sprintSpeed : walkSpeed;

        controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);

        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        // ── MIRAR ────────────────────────────────────────────────────
        float lookX;
        float lookY;

        if (usingStick)
        {
            // Stick/virtual joystick: valores -1 a 1, necesita Time.deltaTime
            lookX = lookInput.x * gamepadSensitivity * Time.deltaTime;
            lookY = lookInput.y * gamepadSensitivity * Time.deltaTime;
        }
        else
        {
            // Mouse: delta ya es frame-relative, no necesita Time.deltaTime
            lookX = lookInput.x * mouseSensitivity;
            lookY = lookInput.y * mouseSensitivity;
        }

        verticalLookRotation -= lookY;
        verticalLookRotation  = Mathf.Clamp(verticalLookRotation, -verticalLookRange, verticalLookRange);
        cameraTransform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
        transform.Rotate(Vector3.up * lookX);

        // ── BOB + PASOS ──────────────────────────────────────────────
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

    // ── CAMERA BOB ───────────────────────────────────────────────────
    private void HandleCameraBob()
    {
        float freq = sprintInput ? sprintBobFrequency  : walkBobFrequency;
        float ampY = sprintInput ? sprintBobAmplitudeY : walkBobAmplitudeY;
        float ampX = sprintInput ? sprintBobAmplitudeX : walkBobAmplitudeX;

        bobTimer += Time.deltaTime * freq;

        Vector3 bobOffset = new Vector3(
            Mathf.Cos(bobTimer * 0.5f)       * ampX,
            Mathf.Abs(Mathf.Sin(bobTimer))   * ampY,
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

    // ── FOOTSTEPS ────────────────────────────────────────────────────
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
        if (clips == null || clips.Length == 0) return;

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