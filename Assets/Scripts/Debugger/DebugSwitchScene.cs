using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugSwitchScene : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void SwitchScene()
    {
        GameManager.Instance.playerCount = 1;
        GameManager.Instance.isOnKeyboard = true;
        SceneManager.LoadScene(sceneName);
    }
}
