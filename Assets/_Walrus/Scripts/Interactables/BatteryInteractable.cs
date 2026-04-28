using UnityEngine;

public class BatteryInteractable : InteractableObject
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public override void Interact()
    {
        base.Interact(); // Debug

        InventoryManager.Instance.PickupBattery();
        Destroy(gameObject);
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