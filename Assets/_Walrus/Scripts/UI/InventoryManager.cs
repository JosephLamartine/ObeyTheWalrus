using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    [Header("─── Inventory Sounds ────────────────────")]
    public AudioClip sndPickupItem;
    public AudioClip sndBatterySwap;
    public AudioClip sndPickupKey;
    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ═══════════════════════════════════════════
    // FLASHLIGHT
    // ═══════════════════════════════════════════
    [Header("─── Flashlight ────────────────────")]
    [SerializeField] private bool  _hasFlashlight = false;
    [SerializeField] private bool  _flashlightOn  = false;
    [SerializeField] [Range(0f,1f)] private float _batteryLevel = 1f;
    [SerializeField] private int   _batteryCount  = 0;

    public bool  hasFlashlight => _hasFlashlight;
    public bool  flashlightOn  => _flashlightOn;
    public float batteryLevel  => _batteryLevel;
    public int   batteryCount  => _batteryCount;

    public event Action<float> OnBatteryChanged;
    public event Action<bool>  OnFlashlightToggled;
    public event Action OnFlashlightPickedUp;
    public event Action        OnInventoryChanged;
    public event Action        OnBatterySwapped;

    public void PickupFlashlight()
    {
        _hasFlashlight = true;
        _batteryLevel  = 1f;
        OnFlashlightPickedUp?.Invoke();
        OnInventoryChanged?.Invoke();
        AudioManager.Instance.PlaySFX(sndPickupItem);
    }

    public void ToggleFlashlight()
    {
        if (!_hasFlashlight || _batteryLevel <= 0f) return;
        _flashlightOn = !_flashlightOn;
        OnFlashlightToggled?.Invoke(_flashlightOn);
    }

    public void DrainBattery(float amountPerSecond)
    {
        _batteryLevel = Mathf.Clamp01(_batteryLevel - amountPerSecond * Time.deltaTime);
        OnBatteryChanged?.Invoke(_batteryLevel);

        if (_batteryLevel <= 0f)
        {
            _flashlightOn = false;
            OnFlashlightToggled?.Invoke(false);

            if (_batteryCount > 0)
            {
                _batteryCount--;
                _batteryLevel = 1f;
                _flashlightOn = true;
                OnFlashlightToggled?.Invoke(true);
                OnBatteryChanged?.Invoke(_batteryLevel);
                OnInventoryChanged?.Invoke();
                OnBatterySwapped?.Invoke();
                AudioManager.Instance.PlaySFX(sndBatterySwap);
            }
        }
    }

    public void PickupBattery()
    {
        _batteryCount++;
        OnInventoryChanged?.Invoke();
        AudioManager.Instance.PlaySFX(sndPickupItem);
    }

    // ═══════════════════════════════════════════
    // LLAVES
    // ═══════════════════════════════════════════
    [Header("─── Llaves ─────────────────────────")]
    [SerializeField] private List<string> _keys = new List<string>(); // visible en Inspector

    public void AddKey(string keyID)
    {
        if (!_keys.Contains(keyID)) _keys.Add(keyID);
        OnInventoryChanged?.Invoke();
    }

    public bool HasKey(string keyID) => _keys.Contains(keyID);

    public void UseKey(string keyID)
    {
        _keys.Remove(keyID);
        OnInventoryChanged?.Invoke();
    }

    // ═══════════════════════════════════════════
    // MAPA
    // ═══════════════════════════════════════════
    [Header("─── Mapa & Ritual ──────────────────")]
    [SerializeField] private bool _hasMap     = false;
    [SerializeField] private bool _hasBlubber = false;
    [SerializeField] private bool _hasAyers   = false;
    [SerializeField] private bool _hasTusk    = false;

    public bool hasMap     => _hasMap;
    public bool hasBlubber => _hasBlubber;
    public bool hasAyers   => _hasAyers;
    public bool hasTusk    => _hasTusk;
    public bool HasAllRitualItems => _hasBlubber && _hasAyers && _hasTusk;

    public void PickupMap()    { _hasMap     = true; OnInventoryChanged?.Invoke(); }
    public void PickupBlubber(){ _hasBlubber = true; OnInventoryChanged?.Invoke(); }
    public void PickupAyers()  { _hasAyers   = true; OnInventoryChanged?.Invoke(); }
    public void PickupTusk()   { _hasTusk    = true; OnInventoryChanged?.Invoke(); }

    // ═══════════════════════════════════════════
    // DEBUG — botones visibles en Inspector (Editor only)
    // ═══════════════════════════════════════════
#if UNITY_EDITOR
    [Header("─── Debug (solo Editor) ───────────")]
    public bool dbg_giveAll      = false;
    public bool dbg_giveFlashlight = false;
    public bool dbg_giveBattery  = false;
    public bool dbg_giveMap      = false;
    public bool dbg_giveRitual   = false;
    public bool dbg_reset        = false;

    private void OnValidate()
    {
        if (dbg_giveAll)       { dbg_giveAll = false;        GiveAll(); }
        if (dbg_giveFlashlight){ dbg_giveFlashlight = false; PickupFlashlight(); }
        if (dbg_giveBattery)   { dbg_giveBattery = false;    PickupBattery(); }
        if (dbg_giveMap)       { dbg_giveMap = false;        PickupMap(); }
        if (dbg_giveRitual)    { dbg_giveRitual = false;     PickupBlubber(); PickupAyers(); PickupTusk(); }
        if (dbg_reset)         { dbg_reset = false;          ResetAll(); }
    }

    private void GiveAll()
    {
        PickupFlashlight();
        PickupBattery(); PickupBattery(); PickupBattery();
        PickupMap();
        PickupBlubber(); PickupAyers(); PickupTusk();
        AddKey("key_heart"); AddKey("key_diamond");
    }

    private void ResetAll()
    {
        _hasFlashlight = false;
        _flashlightOn  = false;
        _batteryLevel  = 1f;
        _batteryCount  = 0;
        _hasMap        = false;
        _hasBlubber    = false;
        _hasAyers      = false;
        _hasTusk       = false;
        _keys.Clear();
        OnInventoryChanged?.Invoke();
    }
#endif
}