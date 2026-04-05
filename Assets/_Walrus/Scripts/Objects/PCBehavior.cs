using System;
using UnityEngine;
using UnityEngine.Video;

public class VideoDevice : MonoBehaviour
{
    [Header("References")]
    public VideoPlayer videoPlayer;
    public Light screenLight;

    [Header("Settings")]
    public bool turnOffLightWhenStopped = true;

    public void StartVideo()
    {
        if (videoPlayer == null) return;

        videoPlayer.Play();
        ParanoiaManager.Instance.StartParanoia();

        if (screenLight != null)
            screenLight.enabled = true;

        Debug.Log($"[VideoDevice] Video iniciado en: {gameObject.name}");
    }
    
    public void PauseVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
            ParanoiaManager.Instance.StopParanoia();
        }
    }
    
    public void StopVideo()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();

        if (screenLight != null && turnOffLightWhenStopped)
            screenLight.enabled = false;

        Debug.Log($"[VideoDevice] Video detenido en: {gameObject.name}");
    }
    
    public bool IsPlaying => videoPlayer != null && videoPlayer.isPlaying;
    
    public bool IsPaused => videoPlayer != null && videoPlayer.isPaused;
    
    public bool IsPrepared => videoPlayer != null && videoPlayer.isPrepared;
}