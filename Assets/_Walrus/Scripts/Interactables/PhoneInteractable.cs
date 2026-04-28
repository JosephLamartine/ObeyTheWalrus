using UnityEngine;

public class PhoneInteractable : InteractableObject
{
    private Animator anim;

    private PhoneBehavior scriptBehavior;
    
    private void Awake()
    {
        anim = GetComponent<Animator>();
        scriptBehavior = GetComponent<PhoneBehavior>();
    }

    public override void Interact()
    {
        base.Interact(); // Debug

        if (canUseIt)
        {
            if (GameManager.Instance.storyStep == 1)
            {
                DialogueManager.Instance.LoadScript(DialogueManager.Instance.jsonPhoneCall1);
                DialogueManager.Instance.StartDialogue();
                DisableInteract();
                scriptBehavior.StopRingtone();
                scriptBehavior.StartCall();
                
                GameManager.Instance.UpdateStoryStep(2);
            }
            
            DisableInteract();
            //anim.SetTrigger("Interact");   
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
