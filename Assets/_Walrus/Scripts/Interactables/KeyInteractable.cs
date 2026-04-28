using UnityEngine;

public class KeyInteractable : InteractableObject
{
    private Animator anim;
    
    [Header("Key Settings")]
    public string keyID;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public override void Interact()
    {
        base.Interact(); // Debug

        InventoryManager.Instance.AddKey(keyID);
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