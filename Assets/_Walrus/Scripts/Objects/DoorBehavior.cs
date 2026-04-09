using UnityEngine;

public class DoorBehavior : MonoBehaviour
{
    private AudioSource au;
    public AudioClip sndCloseDoor;
    public AudioClip sndOpenDoor;
    void Start()
    {
        au = GetComponent<AudioSource>();
    }

    public void CloseDoor()
    {
        au.PlayOneShot(sndCloseDoor);
    }
    
    public void OpenDoor()
    {
        au.PlayOneShot(sndOpenDoor);
    }
}
