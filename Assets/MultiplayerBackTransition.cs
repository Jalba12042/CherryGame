using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MultiplayerBackTransition : MonoBehaviour
{
    [Header("Eraser Transition")]
    public Animator eraserAnimator; // The RandomSlidingWipe(Easer) object
    public string eraserTrigger = "SlideWipe";
    public float wipeAnimationDuration = 1.0f;

    [Header("Scene To Load")]
    public string mainMenuSceneName = "MainMenu";

    // Call this from your "Back" button's OnClick event
    public void GoBackToMainMenu()
    {
        StartCoroutine(BackRoutine());
    }

    private IEnumerator BackRoutine()
    {
        if (eraserAnimator != null)
        {
            // Turn on the eraser and trigger it to wipe the screen
            eraserAnimator.gameObject.SetActive(true);
            eraserAnimator.SetTrigger(eraserTrigger);
        }

        // Wait for the eraser to cover the screen
        yield return new WaitForSeconds(wipeAnimationDuration);

        // Load the Main Menu
        SceneManager.LoadScene(mainMenuSceneName);
    }
}