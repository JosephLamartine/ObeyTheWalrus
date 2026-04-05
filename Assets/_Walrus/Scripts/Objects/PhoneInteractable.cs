using UnityEngine;

public class PhoneInteractable : InteractableObject
{
    private Animator anim;
    
    [Header("Prompt")]
    [SerializeField] private string open_Prompt = "[E] to Answer";
    [SerializeField] private string open_SpanishPrompt = "[E] Contestar";
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

    public override void EnableInteract()
    {
        base.EnableInteract();
    }
    
    public override void DisableInteract()
    {
        base.DisableInteract();
    }
}
