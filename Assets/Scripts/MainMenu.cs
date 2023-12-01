using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void OpenLinkURL(string URL_)
    {
        Application.OpenURL(URL_);
    }

    public void SwitchObjectActiveState(GameObject gameObject_)
    {
        gameObject_.SetActive(!gameObject_.activeSelf);
    }

    public void LoadScene(string sceneName_)
    {
        SceneManager.LoadScene(sceneName_);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
