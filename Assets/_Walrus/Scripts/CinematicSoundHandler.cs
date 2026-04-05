using UnityEngine;

public class CinematicSoundHandler : MonoBehaviour
{
    private AudioSource au;
    
    void Start()
    {
        au = GetComponent<AudioSource>();    
    }

    public void PlaySound(AudioClip clip)
    {
        au.PlayOneShot(clip);
    }
}
