using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void ButtonNewGame()
    {
        SceneManager.LoadScene("House");
    }

}
