using UnityEngine;

public class LSwitchInteractable : InteractableObject
{
    [Header("Prompt")]
    [SerializeField] private string open_Prompt = "[E] to Turn Lights On";
    [SerializeField] private string open_SpanishPrompt = "[E] Prender Luz";
    [Space(15)]
    [SerializeField] private string close_Prompt = "[E] to Turn Lights Off";
    [SerializeField] private string close_SpanishPrompt = "[E] Apagar Luz";
    
    [Header("Settings")]
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
            interactionPrompt = close_Prompt;
        }
        else if (GameManager.Instance.language == GameManager.Language.Spanish)
        {
            interactionPrompt = close_SpanishPrompt;   
        }
    }
    
    public void SwitchToOff()
    {
        if (!canUseIt) {return;} //Prohibited
        
        LightManager.Instance.TurnOffLight(lightToSwitch);
        
        if (GameManager.Instance.language == GameManager.Language.English)
        {
            interactionPrompt = open_Prompt;
        }
        else if (GameManager.Instance.language == GameManager.Language.Spanish)
        {
            interactionPrompt = open_SpanishPrompt;   
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