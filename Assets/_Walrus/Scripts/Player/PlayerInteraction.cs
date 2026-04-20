using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración Horror FPS")]
    [SerializeField] private float interactionDistance = 4.5f;

    [Tooltip("Capas que el raycast debe considerar: Default (paredes) + Interactable")]
    [SerializeField] private LayerMask interactionLayerMask;

    private Camera playerCamera;
    private IInteractable currentInteractable;
    private PlayerInputActions inputActions;
    private bool interactInput;

    private void Awake()
    {
        playerCamera = GetComponent<Camera>() != null ? GetComponent<Camera>() : Camera.main;

        inputActions = new PlayerInputActions();
        inputActions.Player.Interact.performed += ctx => interactInput = true;
        inputActions.Player.Interact.canceled += ctx => interactInput = false;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        CheckForInteractable();

        if (currentInteractable != null && interactInput && currentInteractable.CanInteract)
        {
            currentInteractable.Interact();
            interactInput = false;
        }
    }

    private void CheckForInteractable()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.yellow, 0.1f);

        IInteractable newInteractable = null;

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayerMask))
        {
            newInteractable = hit.collider.GetComponent<IInteractable>();

            if (newInteractable != null && !newInteractable.CanInteract)
                newInteractable = null;
        }

        if (newInteractable != currentInteractable)
        {
            currentInteractable = newInteractable;

            if (currentInteractable != null)
            {
                UI_Interaction.Instance?.Show(
                    currentInteractable.GetInteractionPrompt(),
                    currentInteractable.Type
                );
            }
            else
            {
                UI_Interaction.Instance?.Hide();
            }
        }
    }
}