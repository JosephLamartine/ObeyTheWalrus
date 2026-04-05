using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    [SerializeField] public string interactionPrompt = "Press [E] to Interact";
    [SerializeField] public string spanishPrompt = "[E] para Interactuar";
    [Space(15)]
    [SerializeField] public string prohibitedPrompt = "I don't need it";
    [SerializeField] public string prohibitedSpanishPrompt = "No lo necesito";
    
    [Header("Settings")]
    [SerializeField] public float maxDistance = 4f;
    [SerializeField] public bool canInteract = true;
    [SerializeField] public bool canUseIt = true;
    [SerializeField] private InteractionType interactionType = InteractionType.Interact;

    
    public InteractionType Type => interactionType; 
    public string GetInteractionPrompt() => interactionPrompt;
    public float MaxInteractionDistance => maxDistance;
    public bool CanInteract => canInteract;
    public bool CanUseIt => canUseIt;
    
    public void OnDisable()
    {
        GameManager.Instance.OnLanguageChanged -= EvaluateLanguage;
    }

    private void Start()
    {
        GameManager.Instance.OnLanguageChanged += EvaluateLanguage;
        EvaluateLanguage();
        EvaluateCanUseIt();
    }
    
    private void EvaluateLanguage()
    {
        if (GameManager.Instance.language == GameManager.Language.Spanish)
        {
            interactionPrompt = spanishPrompt;
        }
    }
    
    private void EvaluateCanUseIt()
    {
        if (!canUseIt)
        {
            if (GameManager.Instance.language == GameManager.Language.Spanish)
            {
                interactionPrompt = prohibitedSpanishPrompt;
            }
            else if (GameManager.Instance.language == GameManager.Language.English)
            {
                interactionPrompt = prohibitedPrompt;
            }
        }
    }
    
    public virtual void Interact()
    {
        Debug.Log($"<color=red>INTERACT:</color> {gameObject.name}");
    }
    
    public virtual void EnableInteract()
    {
        canInteract = true;
    }
    
    public virtual void DisableInteract()
    {
        canInteract = false;
    }
}