using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugSwitchScene : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void SwitchScene()
    {
        if (GameManager.Instance != null)
        {
            // Force the game into a 1-player keyboard test mode
            GameManager.Instance.playerCount = 1;
            GameManager.Instance.isOnKeyboard = true;
        }
        else
        {
            Debug.LogWarning("GameManager is missing! Make sure it exists in your persistent scene.");
        }

        SceneManager.LoadScene(sceneName);
    }
}