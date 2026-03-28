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
    [SerializeField] private float jumpHeight = 2.5f;

    [Header("Mirar")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float verticalLookRange = 80f; // ±80°

    // Variables de estado
    private Vector3 playerVelocity;
    private bool isGrounded;
    private float verticalLookRotation = 0f;

    // Input
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool sprintInput;

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        
        // Cursor locked
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Input
        inputActions = new PlayerInputActions();
        
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        
        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;
        
        inputActions.Player.Sprint.performed += ctx => sprintInput = true;
        inputActions.Player.Sprint.canceled += ctx => sprintInput = false;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; // pequeño valor negativo para mantenerlo pegado al suelo
        }

        // ── MOVIMIENTO ───────────────────────────────────────────────
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        float currentSpeed = sprintInput ? sprintSpeed : walkSpeed;

        Vector3 move = moveDirection.normalized * currentSpeed * Time.deltaTime;
        controller.Move(move);

        // ── GRAVEDAD y SALTO (opcional, descomenta si quieres salto) ──
        // if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        // {
        //     playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        // }

        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        // ── MIRAR ─────────────────────────────────────────────────────
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        // Rotación vertical (cámara)
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -verticalLookRange, verticalLookRange);

        cameraTransform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);

        // Rotación horizontal (cuerpo entero)
        transform.Rotate(Vector3.up * mouseX);
    }

    // Opcional: para debug
    private void OnDrawGizmosSelected()
    {
        if (controller != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + controller.center, controller.radius);
        }
    }
}