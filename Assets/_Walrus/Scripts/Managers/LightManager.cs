using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance { get; private set; }

    [Header("Luces controladas")]
    [SerializeField] private List<Light> allLights = new List<Light>();
    
    private Dictionary<string, Light> lightsByName = new Dictionary<string, Light>();

    [Header("Configuración")]
    public bool disableLightsOnAdditiveLoad = true;

    public AudioClip sndSwitchOn;
    public AudioClip sndSwitchOff;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        BuildLightDictionary(); // Construir el diccionario al inicio
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    // ========================
    // REGISTRO DE LUCES
    // ========================
    private void BuildLightDictionary()
    {
        lightsByName.Clear();
        foreach (var light in allLights)
        {
            if (light != null && !lightsByName.ContainsKey(light.name))
            {
                lightsByName[light.name] = light;
            }
        }
    }

    public void RegisterLights(Light[] lights)
    {
        foreach (var light in lights)
        {
            if (light != null && !allLights.Contains(light))
            {
                allLights.Add(light);
                if (!lightsByName.ContainsKey(light.name))
                    lightsByName[light.name] = light;
            }
        }
    }

    public void RegisterLightsFromRoot(GameObject root)
    {
        if (root == null) return;
        Light[] lights = root.GetComponentsInChildren<Light>(true);
        RegisterLights(lights);
    }
    
    public void TurnOnLight(string lightName)
    {
        if (lightsByName.TryGetValue(lightName, out Light light))
        {
            if (light != null)
            {
                light.enabled = true;
                AudioManager.Instance.PlaySFX(sndSwitchOn);
            }
        }
        else
        {
            Debug.LogWarning($"[LightManager] Luz no encontrada: {lightName}");
        }
    }

    /// <summary>
    /// Apaga una luz específica por su nombre
    /// </summary>
    public void TurnOffLight(string lightName)
    {
        if (lightsByName.TryGetValue(lightName, out Light light))
        {
            if (light != null)
            {
                light.enabled = false;
                AudioManager.Instance.PlaySFX(sndSwitchOff);
            }
        }
        else
        {
            Debug.LogWarning($"[LightManager] Luz no encontrada: {lightName}");
        }
    }

    /// <summary>
    /// Cambia el estado de una luz (toggle)
    /// </summary>
    public void ToggleLight(string lightName)
    {
        if (lightsByName.TryGetValue(lightName, out Light light))
        {
            if (light != null)
                light.enabled = !light.enabled;
        }
    }

    /// <summary>
    /// Cambia la intensidad de una luz específica
    /// </summary>
    public void SetLightIntensity(string lightName, float intensity)
    {
        if (lightsByName.TryGetValue(lightName, out Light light))
        {
            if (light != null)
                light.intensity = intensity;
        }
    }

    // ========================
    // MÉTODOS GLOBALES (mantengo los anteriores)
    // ========================
    public void TurnOnAll()
    {
        SetAllLights(true);
    }

    public void TurnOffAll()
    {
        SetAllLights(false);
    }

    private void SetAllLights(bool enabled)
    {
        foreach (var light in allLights)
        {
            if (light != null)
                light.enabled = enabled;
        }
    }

    public void ToggleAll()
    {
        if (allLights.Count > 0)
            SetAllLights(!allLights[0].enabled);
    }

    // ========================
    // EVENTOS DE ESCENAS
    // ========================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive)
        {
            Light[] newLights = FindObjectsOfType<Light>(true);
            RegisterLights(newLights);

            if (disableLightsOnAdditiveLoad)
                Invoke("TurnOffAll", 0.1f);
        }
        else if (mode == LoadSceneMode.Single)
        {
            // Si cargas una escena normal, reconstruye el diccionario
            BuildLightDictionary();
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        // Opcional: limpiar luces nulas
        allLights.RemoveAll(light => light == null);
        BuildLightDictionary();
    }
}
