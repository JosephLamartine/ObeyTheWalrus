using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class InteractionGate : MonoBehaviour
{
    public PlayableDirector director;
    public GameObject button;

    public void PlayDirector()
    {
        director.Play();
        button.SetActive(false);
    }

}
