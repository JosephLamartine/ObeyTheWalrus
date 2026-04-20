using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractableObject : MonoBehaviour, IInteractable
{
    public enum Indicator
    {
        None,
        Show,
        Hide
    }
    
    [Header("Prompt")]
    [SerializeField] public string interactionPrompt = "Press [E] to Interact";
    [SerializeField] public string spanishPrompt = "[E] para Interactuar";
    
    [Header("Settings")]
    [SerializeField] public float maxDistance = 4f;
    [SerializeField] public bool canInteract = true;
    [SerializeField] public bool canUseIt = true;
    [SerializeField] private InteractionType interactionType = InteractionType.Interact;

    [Header("Indicators")]
    public Indicator indicator = Indicator.None;
    public IndicatorsHandler indicatorsHandler;
    public int indicatorIndex = 0;
    
    public InteractionType Type => interactionType; 
    public string GetInteractionPrompt() => interactionPrompt;
    public float MaxInteractionDistance => maxDistance;
    public bool CanInteract => canInteract;
    
    public void OnDisable()
    {
        GameManager.Instance.OnLanguageChanged -= EvaluateLanguage;
    }

    private void Start()
    {
        GameManager.Instance.OnLanguageChanged += EvaluateLanguage;
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
        
        if (indicator != Indicator.None)
        {
            switch (indicator)
            {
                case Indicator.Show:
                    indicatorsHandler.indicator[indicatorIndex].SetActive(true);
                    break;
                case Indicator.Hide:
                    indicatorsHandler.indicator[indicatorIndex].SetActive(false);
                    break;
            }
        }
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