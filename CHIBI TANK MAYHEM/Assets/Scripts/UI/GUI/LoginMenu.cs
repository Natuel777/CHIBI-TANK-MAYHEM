using UnityEngine;
using UnityEngine.SceneManagement; 

public class LoginMenu : MonoBehaviour
{
    //Primero es una pantalla splash, luego será un login real con Playfab
    
    public void GoToMainMenu() {SceneManager.LoadScene("MainMenu");}
}