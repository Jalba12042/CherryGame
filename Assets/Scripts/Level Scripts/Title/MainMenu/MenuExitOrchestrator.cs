using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuExitOrchestrator : MonoBehaviour
{
    [Header("All pieces that should animate out")]
    public Animator[] exitAnimators;
    public string exitTrigger = "Exit";

    [Header("Optional: lock input while exiting")]
    public MonoBehaviour menuControllerToDisable;

    [Header("--- LOCAL: Paper Setup ---")]
    public Animator paperTransitionAnimator;
    [Tooltip("Type PaperOpen here!")]
    public string paperTriggerName = "PaperOpen";
    public float delayBeforePaperStarts = 1.0f;
    public float paperCoverDuration = 1.2f;

    [Header("--- ONLINE: Beach Setup ---")]
    public Animator beachWipeAnimator;
    public string wipeTriggerName = "SlideWipe";
    public float delayBeforeBeachStarts = 1.0f;
    public float beachCoverDuration = 2.0f;

    [Header("--- ONLINE: Snow Setup ---")]
    public Animator snowWipeAnimator;
    public float delayBeforeSnowStarts = 1.0f;
    public float snowCoverDuration = 2.0f;

    void Awake()
    {
        // --- NEW: Auto-find the controller so it always locks automatically! ---
        if (menuControllerToDisable == null)
        {
            menuControllerToDisable = GetComponent<MainMenuController>();
        }
    }

    public void ExitThenLoad(string sceneName)
    {
        StartCoroutine(Routine(sceneName, false));
    }

    public void ExitToMultiplayer(string sceneName)
    {
        StartCoroutine(Routine(sceneName, true));
    }

    private IEnumerator Routine(string sceneName, bool isMultiplayer)
    {
        // Instantly turn off the inputs so no more sounds can play!
        if (menuControllerToDisable != null)
            menuControllerToDisable.enabled = false;

        // 1. Tell the signs and hill to drop down
        foreach (var anim in exitAnimators)
        {
            if (anim != null) anim.SetTrigger(exitTrigger);
        }

        // 2. Handle the transitions WITH TRIGGERS
        if (isMultiplayer)
        {
            int randomBG = Random.Range(0, 2);
            PlayerPrefs.SetInt("MultiplayerBG", randomBG);
            PlayerPrefs.Save();

            if (randomBG == 0) // BEACH SELECTED
            {
                yield return new WaitForSeconds(delayBeforeBeachStarts);
                if (beachWipeAnimator != null)
                {
                    beachWipeAnimator.gameObject.SetActive(true);
                    yield return null; // Wake up frame
                    beachWipeAnimator.SetTrigger(wipeTriggerName);
                }
                yield return new WaitForSeconds(beachCoverDuration);
            }
            else // SNOW SELECTED
            {
                yield return new WaitForSeconds(delayBeforeSnowStarts);
                if (snowWipeAnimator != null)
                {
                    snowWipeAnimator.gameObject.SetActive(true);
                    yield return null; // Wake up frame
                    snowWipeAnimator.SetTrigger(wipeTriggerName);
                }
                yield return new WaitForSeconds(snowCoverDuration);
            }
        }
        else
        {
            // PAPER SELECTED (Local)
            yield return new WaitForSeconds(delayBeforePaperStarts);
            if (paperTransitionAnimator != null)
            {
                paperTransitionAnimator.gameObject.SetActive(true);
                yield return null; // Wake up frame
                if (!string.IsNullOrEmpty(paperTriggerName))
                {
                    paperTransitionAnimator.SetTrigger(paperTriggerName);
                }
            }
            yield return new WaitForSeconds(paperCoverDuration);
        }

        // 3. Safely load the new scene behind the cover!
        SceneManager.LoadScene(sceneName);
    }
}