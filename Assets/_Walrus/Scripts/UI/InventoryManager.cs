using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ==================== ITEMS ====================
    public bool hasFlashlight { get; private set; } = false;
    public bool hasMap        { get; private set; } = false;
    public float batteryLevel { get; private set; } = 1f; // 0 a 1

    private HashSet<string> keys = new HashSet<string>(); // IDs de llaves

    // ==================== EVENTOS ====================
    public event Action OnInventoryChanged;         // cualquier cambio
    public event Action<float> OnBatteryChanged;    // para actualizar UI batería
    public event Action<bool> OnFlashlightToggled;  // true = encendida

    // ==================== FLASHLIGHT ====================
    private bool flashlightOn = false;

    public void PickupFlashlight()
    {
        hasFlashlight = true;
        batteryLevel  = 1f;
        OnInventoryChanged?.Invoke();
    }

    public void ToggleFlashlight()
    {
        if (!hasFlashlight || batteryLevel <= 0f) return;
        flashlightOn = !flashlightOn;
        OnFlashlightToggled?.Invoke(flashlightOn);
    }

    public bool IsFlashlightOn() => flashlightOn;

    // Llamalo desde FlashlightBehavior en Update cuando está encendida
    public void DrainBattery(float amount)
    {
        batteryLevel = Mathf.Clamp01(batteryLevel - amount);
        OnBatteryChanged?.Invoke(batteryLevel);

        if (batteryLevel <= 0f)
        {
            flashlightOn = false;
            OnFlashlightToggled?.Invoke(false);
        }
    }

    public void AddBattery(float amount)
    {
        batteryLevel = Mathf.Clamp01(batteryLevel + amount);
        OnBatteryChanged?.Invoke(batteryLevel);
        OnInventoryChanged?.Invoke();
    }

    // ==================== LLAVES ====================
    public void AddKey(string keyID)
    {
        keys.Add(keyID);
        OnInventoryChanged?.Invoke();
        Debug.Log($"[Inventory] Llave obtenida: {keyID}");
    }

    public bool HasKey(string keyID) => keys.Contains(keyID);

    public void UseKey(string keyID)
    {
        keys.Remove(keyID);
        OnInventoryChanged?.Invoke();
        Debug.Log($"[Inventory] Llave usada: {keyID}");
    }

    // ==================== MAPA ====================
    public void PickupMap()
    {
        hasMap = true;
        OnInventoryChanged?.Invoke();
    }

    // ==================== SHOW / HIDE ====================
    public event Action OnShowInventory;
    public event Action OnHideInventory;

    public void ShowInventory() => OnShowInventory?.Invoke();
    public void HideInventory() => OnHideInventory?.Invoke();
}