using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class OnlineMenuTransition : MonoBehaviour
{
    [Header("Menu Elements To Drop Down")]
    [Tooltip("Drag the Hill, Local, Online, Quit, and Rock animators here!")]
    public Animator[] exitAnimators;
    public string exitTrigger = "Exit";
    public float exitDropDuration = 1.0f;

    [Header("Transition Animators (Right to Left)")]
    public Animator beachWipeAnimator;
    public Animator snowWipeAnimator;

    [Header("Settings")]
    public float wipeAnimationDuration = 1.0f;
    public string multiplayerSceneName = "MultiplayerScene";
    public string triggerName = "SlideWipe";

    public void StartOnlineTransition()
    {
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        // 1. Drop ALL the signs and the hill down!
        if (exitAnimators != null)
        {
            foreach (Animator anim in exitAnimators)
            {
                if (anim != null) anim.SetTrigger(exitTrigger);
            }
        }

        yield return new WaitForSeconds(exitDropDuration);

        // 2. Pick a random background (0 = Beach, 1 = Mountain)
        int randomBG = Random.Range(0, 2);

        // 3. Save this choice so the Multiplayer scene knows which one we picked
        PlayerPrefs.SetInt("MultiplayerBG", randomBG);
        PlayerPrefs.Save();

        // 4. Play the correct transition animation!
        if (randomBG == 0 && beachWipeAnimator != null)
        {
            beachWipeAnimator.gameObject.SetActive(true);
            beachWipeAnimator.SetTrigger(triggerName);
        }
        else if (randomBG == 1 && snowWipeAnimator != null)
        {
            snowWipeAnimator.gameObject.SetActive(true);
            snowWipeAnimator.SetTrigger(triggerName);
        }

        // 5. Wait for the animation to cover the screen
        yield return new WaitForSeconds(wipeAnimationDuration);

        // 6. Load the multiplayer scene!
        SceneManager.LoadScene(multiplayerSceneName);
    }
}