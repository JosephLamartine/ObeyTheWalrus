using UnityEngine;

public class DoorInteractable : InteractableObject
{
    private Animator anim;
    
    [Header("Door Settings")]
    public bool isOpen = false;
    
    [Header("Close/Open Prompt")]
    [SerializeField] private string open_Prompt = "Press [E] to Open";
    [SerializeField] private string open_SpanishPrompt = "[E] Abrir Puerta";
    [Space(15)]
    [SerializeField] private string close_Prompt = "Press [E] to Close";
    [SerializeField] private string close_SpanishPrompt = "[E] Cerrar Puerta";

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public override void Interact()
    {
        base.Interact(); // Debug

        if (canUseIt)
        {
            DisableInteract();
            anim.SetTrigger("Interact");   
        }
        else
        {
            DialogueManager.Instance.LoadScript(DialogueManager.Instance.jsonBathroom);
            DialogueManager.Instance.StartDialogue();
        }
    }

    public void SwitchToOpened()
    {
        if (!canUseIt) {return;} //Prohibited
        
        isOpen = false;
        
        if (GameManager.Instance.language == GameManager.Language.English)
        {
            interactionPrompt = close_Prompt;
        }
        else if (GameManager.Instance.language == GameManager.Language.Spanish)
        {
            interactionPrompt = close_SpanishPrompt;   
        }
    }
    
    public void SwitchToClosed()
    {
        if (!canUseIt) {return;} //Prohibited
        
        isOpen = true;
        
        if (GameManager.Instance.language == GameManager.Language.English)
        {
            interactionPrompt = open_Prompt;
        }
        else if (GameManager.Instance.language == GameManager.Language.Spanish)
        {
            interactionPrompt = open_SpanishPrompt;   
        }
    }

    public override void EnableInteract()
    {
        base.EnableInteract();
    }
    
    public override void DisableInteract()
    {
        base.DisableInteract();
    }
}