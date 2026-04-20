using System;
using UnityEngine;

public class PhoneBehavior : MonoBehaviour
{
    private AudioSource au;
    public AudioClip sndRingtone;
    
    private PhoneInteractable scriptInteract;

    private void Start()
    {
        au = GetComponent<AudioSource>();
        scriptInteract = GetComponent<PhoneInteractable>();

        // Mover el registro aquí, cuando DialogueManager.Instance ya existe seguro
        DialogueManager.Instance.RegisterEvent("StartRingtone", StartRingtone);
    }

    private void OnDestroy()
    {
        // OnDestroy en lugar de OnDisable, más seguro para singletons
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.UnregisterEvent("StartRingtone", StartRingtone);
    }

    private void StartRingtone()
    {
        au.clip = sndRingtone;
        au.Play();
        scriptInteract.canInteract = true;
    }

    public void StopRingtone()
    {
        au.Stop();
    }
}
