using UnityEngine;

public class PCInteractable : InteractableObject
{
    private Animator anim;
    private VideoDevice videoDevice;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        videoDevice = GetComponent<VideoDevice>();
    }

    public override void Interact()
    {
        base.Interact(); // Debug
        
        if (GameManager.Instance.storyStep == 0)
        {
            DialogueManager.Instance.LoadScript(DialogueManager.Instance.jsonDialogue1);
            DialogueManager.Instance.StartDialogue();
            videoDevice.PauseVideo();
            DisableInteract();
            GameManager.Instance.UpdateStoryStep(1);
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