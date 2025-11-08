using UnityEngine;
using UnityEngine.SceneManagement;

public class GameExitHandler : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ReturnToMenu();
        }
    }

    public void ReturnToMenu()
    {
        Debug.Log("Returning to menu...");
        SceneManager.LoadScene("MainMenu");  // Go back to main menu
    }
}
