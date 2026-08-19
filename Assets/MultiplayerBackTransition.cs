using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MultiplayerBackTransition : MonoBehaviour
{
    public Animator eraserAnimator;
    public string eraserTrigger = "SlideWipe";
    public float wipeAnimationDuration = 1f;
    public string sceneToLoad = "Main Menu";

    [Header("Lock Input During Transition")]
    [Tooltip("The controller script to disable so no more sounds play.")]
    public MonoBehaviour menuControllerToDisable;

    void Awake()
    {
        // Auto-find the MainMenuController in the scene so we can lock it automatically!
        if (menuControllerToDisable == null)
        {
            menuControllerToDisable = Object.FindFirstObjectByType<MainMenuController>();
        }
    }

    // We will call this function from your buttons and your Menu Controller!
    public void PlayBackTransition()
    {
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        // --- NEW: Instantly freeze the inputs so UI sounds stop playing! ---
        if (menuControllerToDisable != null)
        {
            menuControllerToDisable.enabled = false;
        }

        if (eraserAnimator != null)
        {
            eraserAnimator.gameObject.SetActive(true);
            yield return null; // Wait 1 frame for Animator to wake up
            eraserAnimator.SetTrigger(eraserTrigger);
            yield return new WaitForSeconds(wipeAnimationDuration);
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}