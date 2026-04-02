using UnityEngine;
using System;
public class GameManager : MonoBehaviour
{
    // ==================== SINGLETON ====================
    public static GameManager Instance { get; private set; }

    // ==================== ESTADO ====================
    public bool IsPaused { get; private set; } = false;

    public enum Language
    {
        English,
        Spanish
    }
    
    public Language language = Language.English;
    
    public Action OnLanguageChanged;
    private void Awake()
    {
        // Singleton simple y seguro
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ==================== PAUSA ====================
    public void PauseGame()
    {
        if (IsPaused) return;

        IsPaused = true;
        Time.timeScale = 0f;
        Debug.Log("Juego Pausado");
    }

    public void ResumeGame()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = 1f;
        Debug.Log("Juego Reanudado");
    }

    // Toggle (útil para presionar Escape)
    public void TogglePause()
    {
        if (IsPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void ChangeLanguage(Language newLanguage)
    {
        language = newLanguage;
        OnLanguageChanged?.Invoke();
    }
}