using UnityEngine;

public class DoorInteractable : InteractableObject
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public override void Interact()
    {
        base.Interact(); // Debug
        DisableInteract();
        anim.SetTrigger("Interact");
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