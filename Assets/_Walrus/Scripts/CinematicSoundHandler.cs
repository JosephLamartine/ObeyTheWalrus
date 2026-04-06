using UnityEngine;

public class CinematicSoundHandler : MonoBehaviour
{
    public void PlaySound(AudioClip clip)
    {
        AudioManager.Instance.PlaySFX(clip);
    }
}
