using UnityEngine;

public class UI_Master : MonoBehaviour
{
    public static UI_Master Instance { get; private set; }

    [Header("UI Controls")]
    [SerializeField] private CanvasGroup controlHUD;
    
    [Header("UI Inventory")]
    [SerializeField] private CanvasGroup inventoryHUD;
    
    [Header("Platform UI")]
    [SerializeField] private GameObject mobileControlsUI;

    private bool isAndroidPlatform;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        isAndroidPlatform = Application.platform == RuntimePlatform.Android;
    }

    private void Start()
    {
        SetupPlatformUI();
        ShowControlHUD();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void SetupPlatformUI()
    {
        if (mobileControlsUI != null)
            mobileControlsUI.SetActive(isAndroidPlatform);
    }

    // =====================

    public void ShowControlHUD()
    {
        if (controlHUD == null) return;

        controlHUD.alpha = 1;
        controlHUD.blocksRaycasts = true;
        controlHUD.interactable = true;
    }

    public void HideControlHUD()
    {
        if (controlHUD == null) return;

        controlHUD.alpha = 0;
        controlHUD.blocksRaycasts = false;
        controlHUD.interactable = false;
    }
    
    // =====================

    public void ShowInventoryHUD()
    {
        if (inventoryHUD == null) return;

        inventoryHUD.alpha = 1;
        inventoryHUD.blocksRaycasts = true;
        inventoryHUD.interactable = true;
    }

    public void HideInventoryHUD()
    {
        if (inventoryHUD == null) return;

        inventoryHUD.alpha = 0;
        inventoryHUD.blocksRaycasts = false;
        inventoryHUD.interactable = false;
    }
}