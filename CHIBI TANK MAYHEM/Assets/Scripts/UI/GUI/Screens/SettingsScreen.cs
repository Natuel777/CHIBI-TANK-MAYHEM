using UnityEngine;

public class SettingsScreen : Screen
{
    public void BackToMainManu()
    {
        if(backScreen != null)
        {
            ScreenManager.Instance.DesactiveScreen();
            ScreenManager.Instance.ActiveScreen(backScreen);
        }
    }
}
