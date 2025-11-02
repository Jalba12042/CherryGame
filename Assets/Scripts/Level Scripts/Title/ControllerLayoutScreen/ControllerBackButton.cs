using UnityEngine;
using UnityEngine.SceneManagement;

public class ControllerBackButton : MonoBehaviour
{
    public void GoBack()
    {
        if (!string.IsNullOrEmpty(SceneReturnManager.previousScene))
        {
            SceneManager.LoadScene(SceneReturnManager.previousScene);
        }
        else
        {
            // fallback if something went wrong
            SceneManager.LoadScene("MainMenu");
        }
    }
}

