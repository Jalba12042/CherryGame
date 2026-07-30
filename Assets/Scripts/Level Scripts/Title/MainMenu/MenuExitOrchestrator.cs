using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MenuExitOrchestrator : MonoBehaviour
{
    [Header("All pieces that should animate out")]
    public Animator[] exitAnimators;      // Hill, Clouds, LocalButton, Rock(Controller), Multiplayer
    public string exitTrigger = "Exit";

    [Header("Timing")]
    public float exitDuration = 1.14f;      // set to your longest leave clip length

    [Header("Optional: lock input while exiting")]
    public MonoBehaviour menuControllerToDisable;  // assign your MainMenuController

    [Header("Paper Transition Setup")]
    public Animator paperTransitionAnimator;
    public float paperAnimationDuration = 1f;

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
        // 1. Wait for the menu pieces to leave
        yield return new WaitForSeconds(exitDuration);

        // 2. Trigger the paper wipe
        if (paperTransitionAnimator != null)
        {
            Image paperImg = paperTransitionAnimator.GetComponent<Image>();
            if (paperImg != null)
            {
                Color c = paperImg.color;
                c.a = 1f;
                paperImg.color = c;
            }

            paperTransitionAnimator.SetTrigger("PaperOpen");

            // 3. Wait for paper to cover the screen
            yield return new WaitForSeconds(paperAnimationDuration);
        }

        // 4. Load the scene!
        SceneManager.LoadScene(sceneName);
    }
}