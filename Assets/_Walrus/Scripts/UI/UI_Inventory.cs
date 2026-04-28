using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// OBEDECE A LA MORSA — InventoryUI
///
/// Conecta los slots del Canvas con el InventoryManager.
/// Todo se asigna desde el Inspector. Sin lógica de juego acá.
///
/// SETUP:
///   Agrega este script a cualquier GameObject del Canvas.
///   Arrastra cada slot desde la jerarquía del Canvas a los campos de abajo.
/// </summary>
public class UI_Inventory : MonoBehaviour
{
    // ═══════════════════════════════════════════
    // FLASHLIGHT SLOT
    // ═══════════════════════════════════════════
    [Header("─── Flashlight ────────────────────")]
    [Tooltip("El GameObject del slot completo (se activa al recoger la linterna)")]
    public GameObject flashlightSlot;

    [Tooltip("Ícono cuando la linterna está APAGADA")]
    public Sprite iconFlashlightOff;

    [Tooltip("Ícono cuando la linterna está ENCENDIDA")]
    public Sprite iconFlashlightOn;

    [Tooltip("Image del ícono de la linterna")]
    public Image flashlightIcon;

    [Tooltip("Slider que muestra el % de batería")]
    public Slider batteryBar;

    [Tooltip("Texto que muestra el % numerico, ej '73%' (opcional)")]
    public TextMeshProUGUI batteryText;

    // ═══════════════════════════════════════════
    // BATERÍAS SLOT
    // ═══════════════════════════════════════════
    [Header("─── Baterías ───────────────────────")]
    [Tooltip("El GameObject del slot de baterías (siempre visible o solo si tiene alguna)")]
    public GameObject batterySlot;

    [Tooltip("Texto que muestra la cantidad, ej 'x3'")]
    public TextMeshProUGUI batteryCountText;

    [Tooltip("Sonido al cambiar batería automáticamente")]
    public AudioSource batterySwapAudio;
    public AudioClip   batterySwapClip;

    // ═══════════════════════════════════════════
    // LLAVES — slots individuales por ID
    // ═══════════════════════════════════════════
    [Header("─── Llaves ─────────────────────────")]
    [Tooltip("Define un slot por cada llave posible en el juego")]
    public KeySlot[] keySlots;

    // ═══════════════════════════════════════════
    // MAPA SLOT
    // ═══════════════════════════════════════════
    [Header("─── Mapa ───────────────────────────")]
    public GameObject mapSlot;

    // ═══════════════════════════════════════════
    // RITUAL SLOTS
    // ═══════════════════════════════════════════
    [Header("─── Objetos del Ritual ──────────────")]
    public GameObject blubberSlot;
    public GameObject ayersSlot;
    public GameObject tuskSlot;

    // ═══════════════════════════════════════════
    // LIFECYCLE
    // ═══════════════════════════════════════════

    private void Start()
    {
        // Suscribirse a los eventos del manager
        var inv = InventoryManager.Instance;
        if (inv == null) { Debug.LogError("[InventoryUI] InventoryManager no encontrado"); return; }

        inv.OnInventoryChanged  += RefreshAll;
        inv.OnBatteryChanged    += RefreshBattery;
        inv.OnFlashlightToggled += RefreshFlashlightIcon;
        inv.OnBatterySwapped    += OnBatterySwapped;

        // Estado inicial
        RefreshAll();
    }

    private void OnDestroy()
    {
        var inv = InventoryManager.Instance;
        if (inv == null) return;

        inv.OnInventoryChanged  -= RefreshAll;
        inv.OnBatteryChanged    -= RefreshBattery;
        inv.OnFlashlightToggled -= RefreshFlashlightIcon;
        inv.OnBatterySwapped    -= OnBatterySwapped;
    }

    // ═══════════════════════════════════════════
    // REFRESH
    // ═══════════════════════════════════════════

    private void RefreshAll()
    {
        var inv = InventoryManager.Instance;
        if (inv == null) return;

        // Flashlight slot — aparece al recogerla
        if (flashlightSlot != null)
            flashlightSlot.SetActive(inv.hasFlashlight);

        RefreshBattery(inv.batteryLevel);
        RefreshFlashlightIcon(inv.flashlightOn);

        // Baterías
        if (batterySlot != null)
            batterySlot.SetActive(inv.batteryCount > 0);
        if (batteryCountText != null)
            batteryCountText.text = $"x{inv.batteryCount}";

        // Mapa
        if (mapSlot != null)
            mapSlot.SetActive(inv.hasMap);

        // Llaves
        foreach (var slot in keySlots)
        {
            if (slot.slotObject != null)
                slot.slotObject.SetActive(inv.HasKey(slot.keyID));
        }

        // Ritual
        if (blubberSlot != null) blubberSlot.SetActive(inv.hasBlubber);
        if (ayersSlot   != null) ayersSlot.SetActive(inv.hasAyers);
        if (tuskSlot    != null) tuskSlot.SetActive(inv.hasTusk);
    }

    private void RefreshBattery(float level)
    {
        if (batteryBar != null)
            batteryBar.value = level;

        if (batteryText != null)
            batteryText.text = $"{Mathf.RoundToInt(level * 100f)}%";
    }

    private void RefreshFlashlightIcon(bool isOn)
    {
        if (flashlightIcon == null) return;

        if (isOn && iconFlashlightOn != null)
            flashlightIcon.sprite = iconFlashlightOn;
        else if (!isOn && iconFlashlightOff != null)
            flashlightIcon.sprite = iconFlashlightOff;
    }

    private void OnBatterySwapped()
    {
        if (batterySwapAudio != null && batterySwapClip != null)
            batterySwapAudio.PlayOneShot(batterySwapClip);

        // Refresca el contador de baterías
        var inv = InventoryManager.Instance;
        if (inv != null && batteryCountText != null)
            batteryCountText.text = $"x{inv.batteryCount}";
    }
}

// ─────────────────────────────────────────────────────────────
// KeySlot — un slot de UI por cada tipo de llave
// Configura en el Inspector: keyID + el GameObject del slot
// ─────────────────────────────────────────────────────────────
[System.Serializable]
public class KeySlot
{
    [Tooltip("ID de la llave — debe coincidir con lo que pases a AddKey(keyID)")]
    public string keyID;

    [Tooltip("El GameObject del slot en el Canvas (se activa/desactiva)")]
    public GameObject slotObject;
}