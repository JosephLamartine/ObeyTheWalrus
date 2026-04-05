using UnityEngine;
using UnityEngine.UI;   // <-- agregar
using TMPro;
using System.Collections;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración Horror FPS")]
    [SerializeField] private float interactionDistance = 4.5f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("UI Prompt")]
    [SerializeField] private CanvasGroup promptCanvasGroup;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Image interactionIcon;   

    [Header("Interaction Icons")]
    [SerializeField] private Sprite iconInteract;   
    [SerializeField] private Sprite iconGrab;
    [SerializeField] private Sprite iconExamine;

    [Header("Fade Settings")]
    [SerializeField] private float fadeSpeed = 8f;

    private Camera playerCamera;
    private IInteractable currentInteractable;
    private Coroutine fadeCoroutine;

    private PlayerInputActions inputActions;
    private bool interactInput;

    private void Awake()
    {
        playerCamera = GetComponent<Camera>() != null ? GetComponent<Camera>() : Camera.main;

        inputActions = new PlayerInputActions();
        inputActions.Player.Interact.performed += ctx => interactInput = true;
        inputActions.Player.Interact.canceled  += ctx => interactInput = false;

        if (promptCanvasGroup != null)
            promptCanvasGroup.alpha = 0f;
    }

    private void OnEnable()  => inputActions.Enable();
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
        IInteractable lookedAt = null;
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer))
            lookedAt = hit.collider.GetComponent<IInteractable>();

        if (lookedAt != null && lookedAt.CanInteract)
        {
            if (lookedAt != currentInteractable)
            {
                currentInteractable = lookedAt;
                ShowPrompt(lookedAt.GetInteractionPrompt(), lookedAt.Type);  // <-- pasa el tipo
            }
        }
        else
        {
            if (currentInteractable != null)
            {
                currentInteractable = null;
                HidePrompt();
            }
        }
    }

    private void ShowPrompt(string text, InteractionType type)
    {
        if (promptText != null)
            promptText.text = text;

        // Swappear ícono según tipo
        if (interactionIcon != null)
            interactionIcon.sprite = GetSpriteForType(type);

        if (promptCanvasGroup != null)
            StartFade(1f);
    }

    private Sprite GetSpriteForType(InteractionType type) => type switch
    {
        InteractionType.Grab    => iconGrab,
        InteractionType.Examine => iconExamine,
        _                       => iconInteract
    };

    private void HidePrompt()
    {
        if (promptCanvasGroup != null)
            StartFade(0f);
    }

    private void StartFade(float targetAlpha)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCanvas(targetAlpha));
    }

    private IEnumerator FadeCanvas(float targetAlpha)
    {
        float startAlpha = promptCanvasGroup.alpha;
        float elapsed    = 0f;

        while (!Mathf.Approximately(promptCanvasGroup.alpha, targetAlpha))
        {
            elapsed += Time.deltaTime;
            promptCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed * fadeSpeed);
            yield return null;
        }

        promptCanvasGroup.alpha = targetAlpha;
    }
}