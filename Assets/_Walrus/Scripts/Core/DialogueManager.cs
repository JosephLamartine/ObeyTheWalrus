using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using TMPro;

[Serializable]
public class DialogueEntry
{
    public string text;
    public string audioClip;
    public string animationClip;
    public string cinematicClip;
    public string advanceMode = "audio";
    public string onCompleteTrigger;
}

[Serializable]
public class DialogueScript
{
    public List<DialogueEntry> dialogues = new List<DialogueEntry>();
}

public class DialogueManager : MonoBehaviour
{
    // ==================== SINGLETON PERSISTENTE ====================
    public static DialogueManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (dialogueText != null)
            dialogueText.text = "";
    }

    // ==================== REFERENCIAS ====================
    [Header("UI Dialogue")]
    [Tooltip("El Panel completo que contiene el fondo, texto, retrato, etc.")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Audio y Animaciones")]
    public AudioSource dialogueAudioSource;
    public Animation dialogueAnimation;
    public PlayableDirector cinematicDirector;

    [Header("JSON")]
    public TextAsset jsonDialogue1;
    public TextAsset jsonBathroom;
    public TextAsset currentDialogueAsset;

    // ==================== VARIABLES INTERNAS ====================
    private List<DialogueEntry> currentScript;
    private int currentIndex = 0;
    private Coroutine currentCoroutine;

    // ==================== SISTEMA DE EVENTOS ====================
    private Dictionary<string, Action> eventRegistry = new Dictionary<string, Action>();

    /// <summary>
    /// Registrá un evento por nombre. Debe coincidir con onCompleteTrigger en el JSON.
    /// </summary>
    public void RegisterEvent(string triggerName, Action action)
    {
        if (eventRegistry.ContainsKey(triggerName))
            eventRegistry[triggerName] += action;
        else
            eventRegistry[triggerName] = action;
    }

    /// <summary>
    /// Desregistrá un evento. Llamalo en OnDisable/OnDestroy del script que lo registró.
    /// </summary>
    public void UnregisterEvent(string triggerName, Action action)
    {
        if (eventRegistry.ContainsKey(triggerName))
            eventRegistry[triggerName] -= action;
    }

    private void TriggerOnComplete(string triggerName)
    {
        if (eventRegistry.TryGetValue(triggerName, out Action action))
        {
            Debug.Log($"[Dialogue Event] → {triggerName}");
            action?.Invoke();
        }
        else
        {
            Debug.LogWarning($"[Dialogue Event] Sin registro para: '{triggerName}'");
        }
    }

    // ==================== MÉTODOS PÚBLICOS ====================
    public void LoadScript(TextAsset jsonAsset)
    {
        if (jsonAsset == null)
        {
            Debug.LogError("DialogueManager: JSON nulo");
            return;
        }

        DialogueScript script = JsonUtility.FromJson<DialogueScript>(jsonAsset.text);
        currentScript = script.dialogues;
        currentIndex = 0;

        Debug.Log($"DialogueManager: Cargado {currentScript.Count} diálogos");
    }

    public void StartDialogue(TextAsset jsonAsset)
    {
        LoadScript(jsonAsset);
        StartDialogue();
    }

    public void StartDialogue()
    {
        if (currentScript == null || currentScript.Count == 0)
        {
            if (currentDialogueAsset != null)
                LoadScript(currentDialogueAsset);
            else
            {
                Debug.LogError("DialogueManager: No hay script cargado ni JSON por defecto");
                return;
            }
        }

        currentIndex = 0;
        ShowDialoguePanel(true);
        PlayNextDialogue();
    }

    public void ForceNextDialogue()
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentIndex++;
        PlayNextDialogue();
    }

    // ==================== INTERNOS ====================
    private void ShowDialoguePanel(bool show)
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(show);

        if (!show && dialogueText != null)
            dialogueText.text = "";
    }

    private void PlayNextDialogue()
    {
        if (currentIndex >= currentScript.Count)
        {
            EndDialogue();
            return;
        }

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(PlayCurrentDialogue(currentScript[currentIndex]));
    }

    private IEnumerator PlayCurrentDialogue(DialogueEntry entry)
    {
        if (dialogueText != null)
            dialogueText.text = entry.text;

        AudioClip audioClip = null;
        AnimationClip animClip = null;
        TimelineAsset cineClip = null;

        if (!string.IsNullOrEmpty(entry.audioClip))
        {
            audioClip = Resources.Load<AudioClip>(entry.audioClip);
            if (audioClip != null)
            {
                dialogueAudioSource.clip = audioClip;
                dialogueAudioSource.Play();
            }
        }

        if (!string.IsNullOrEmpty(entry.animationClip) && dialogueAnimation != null)
        {
            animClip = Resources.Load<AnimationClip>(entry.animationClip);
            if (animClip != null)
            {
                dialogueAnimation.clip = animClip;
                dialogueAnimation.Play();
            }
        }

        if (!string.IsNullOrEmpty(entry.cinematicClip) && cinematicDirector != null)
        {
            cineClip = Resources.Load<TimelineAsset>(entry.cinematicClip);
            if (cineClip != null)
            {
                cinematicDirector.playableAsset = cineClip;
                cinematicDirector.Play();
            }
        }

        switch (entry.advanceMode.ToLower())
        {
            case "audio":
                if (audioClip != null)
                    yield return new WaitWhile(() => dialogueAudioSource.isPlaying);
                else
                    yield return new WaitForSeconds(2.5f);
                break;

            case "animation":
                if (animClip != null && dialogueAnimation != null)
                    yield return new WaitWhile(() => dialogueAnimation.isPlaying);
                else
                    yield return new WaitForSeconds(2.5f);
                break;

            case "cinematic":
                if (cineClip != null)
                    yield return new WaitForSeconds((float)cineClip.duration);
                else
                    yield return new WaitForSeconds(2.5f);
                break;

            default:
                yield return new WaitForSeconds(2.5f);
                break;
        }

        if (!string.IsNullOrEmpty(entry.onCompleteTrigger))
            TriggerOnComplete(entry.onCompleteTrigger);

        currentIndex++;
        PlayNextDialogue();
    }

    private void EndDialogue()
    {
        ShowDialoguePanel(false);
        Debug.Log("DialogueManager: Diálogo completado");
    }
}