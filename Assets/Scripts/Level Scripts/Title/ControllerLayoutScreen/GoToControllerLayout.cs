using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToControllerLayout : MonoBehaviour
{
    public void OpenControllerLayout()
    {
        SceneReturnManager.previousScene = SceneManager.GetActiveScene().name; // save origin
        SceneManager.LoadScene("ControllerLayout"); // load your controller scene
    }
}
