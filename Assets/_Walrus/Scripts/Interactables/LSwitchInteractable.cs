using UnityEngine;

public class LSwitchInteractable : InteractableObject
{
    
    [Header("ON/OFF Prompt")]
    [SerializeField] private string ON_Prompt = "[E] to Turn Lights On";
    [SerializeField] private string ON_SpanishPrompt = "[E] Prender Luz";
    [Space(15)]
    [SerializeField] private string OFF_Prompt = "[E] to Turn Lights Off";
    [SerializeField] private string OFF_SpanishPrompt = "[E] Apagar Luz";
    
    [Header("Switch Settings")]
    public bool isOn = false;

    public string lightToSwitch = " ";
    public void Start()
    {
        if (isOn)
        {
            SwitchToOff();
        }
        else
        {
            SwitchToOn();
        }
    }
    

    public override void Interact()
    {
        base.Interact(); // Debug
        isOn = !isOn;

        if (isOn)
        {
            SwitchToOff();
        }
        else
        {
            SwitchToOn();
        }
    }

    public void SwitchToOn()
    {
        if (!canUseIt) {return;} //Prohibited
        
        LightManager.Instance.TurnOnLight(lightToSwitch);
        
        if (GameManager.Instance.language == GameManager.Language.English)
        {
            interactionPrompt = OFF_Prompt;
        }
        else if (GameManager.Instance.language == GameManager.Language.Spanish)
        {
            interactionPrompt = OFF_SpanishPrompt;   
        }
    }
    
    public void SwitchToOff()
    {
        if (!canUseIt) {return;} //Prohibited
        
        LightManager.Instance.TurnOffLight(lightToSwitch);
        
        if (GameManager.Instance.language == GameManager.Language.English)
        {
            interactionPrompt = ON_Prompt;
        }
        else if (GameManager.Instance.language == GameManager.Language.Spanish)
        {
            interactionPrompt = ON_SpanishPrompt;   
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