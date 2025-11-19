using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuExitOrchestrator : MonoBehaviour
{
    [Header("All pieces that should animate out")]
    public Animator[] exitAnimators;      // Hill, Clouds, LocalButton, Rock(Controller), Multiplayer
    public string exitTrigger = "Exit";

    [Header("Timing")]
    public float exitDuration = 0.9f;     // set to your longest leave clip length

    [Header("Optional: lock input while exiting")]
    public MonoBehaviour menuControllerToDisable;  // assign your MainMenuController

    public void ExitThenLoad(string sceneName)
    {
        // stop more input while exiting
        if (menuControllerToDisable) menuControllerToDisable.enabled = false;

        // fire Exit on all animators
        if (exitAnimators != null)
        {
            foreach (var a in exitAnimators)
                if (a) a.SetTrigger(exitTrigger);
        }

        // wait, then change scene
        StartCoroutine(WaitAndLoad(sceneName));
    }

    private IEnumerator WaitAndLoad(string sceneName)
    {
        yield return new WaitForSeconds(exitDuration);
        SceneManager.LoadScene(sceneName);
    }
}
