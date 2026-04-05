using System;
using UnityEngine;

public class PhoneBehavior : MonoBehaviour
{
    private AudioSource au;
    public AudioClip sndCall;
    
    private PhoneInteractable scriptInteract;

    private void Start()
    {
        au = GetComponent<AudioSource>();
        scriptInteract = GetComponent<PhoneInteractable>();

        // Mover el registro aquí, cuando DialogueManager.Instance ya existe seguro
        DialogueManager.Instance.RegisterEvent("Llamada1", OnLlamada1);
    }

    private void OnDestroy()
    {
        // OnDestroy en lugar de OnDisable, más seguro para singletons
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.UnregisterEvent("Llamada1", OnLlamada1);
    }

    private void OnLlamada1()
    {
        au.PlayOneShot(sndCall);
        scriptInteract.canInteract = true;
    }
}
