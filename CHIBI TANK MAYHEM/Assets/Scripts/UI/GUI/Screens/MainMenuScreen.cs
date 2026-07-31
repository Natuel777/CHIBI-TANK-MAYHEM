using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenuScreen : Screen
{
    public void StartGame() {SceneManager.LoadScene("SandBox");}

    private void Start()
    {
        ScreenManager.Instance.ActiveScreen(this);
    }

    public void LoadSettings()
    {
        if(nextScreen != null)
        {
            ScreenManager.Instance.ActiveScreen(nextScreen);
        }
    }

    public void ExitGame() {Application.Quit();}
}
