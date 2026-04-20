using UnityEngine;

public enum InteractionType
{
    Interact,   // ícono genérico de engranaje/mano
    Grab,       // ícono de agarrar/pick up
    Examine     // ícono de ojo/lupa
}

public interface IInteractable
{
    string GetInteractionPrompt();
    void Interact();                   
    InteractionType Type { get; } 
    float MaxInteractionDistance { get; } 
    bool CanInteract { get; }
}