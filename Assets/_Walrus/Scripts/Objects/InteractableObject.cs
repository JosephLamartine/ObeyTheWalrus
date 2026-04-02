using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    [SerializeField] private string interactionPrompt = "Press [E] to Interact";
    [SerializeField] private string spanishPrompt = "[E] para Interactuar";

    [Header("Settings")]
    [SerializeField] private float maxDistance = 4f;
    [SerializeField] private bool canInteract = true;

    public string GetInteractionPrompt() => interactionPrompt;
    public float MaxInteractionDistance => maxDistance;
    public bool CanInteract => canInteract;

    public void OnEnable()
    {
        GameManager.Instance.OnLanguageChanged += EvaluateLanguage;
    }
    
    public void OnDisable()
    {
        GameManager.Instance.OnLanguageChanged -= EvaluateLanguage;
    }

    private void Start()
    {
        EvaluateLanguage();    
    }
    
    private void EvaluateLanguage()
    {
        if (GameManager.Instance.language == GameManager.Language.Spanish)
        {
            interactionPrompt = spanishPrompt;    
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