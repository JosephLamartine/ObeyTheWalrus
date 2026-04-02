using UnityEngine;

public interface IInteractable
{
    string GetInteractionPrompt();
    void Interact();                   
    float MaxInteractionDistance { get; } 
    bool CanInteract { get; }
}