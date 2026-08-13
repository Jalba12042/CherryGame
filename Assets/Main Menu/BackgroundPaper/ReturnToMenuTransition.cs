using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReturnToMenuTransition : MonoBehaviour
{
    [Header("Animators")]
    [Tooltip("The Animator for the cardboard box")]
    public Animator boxAnimator;
    [Tooltip("The Animator for the paper transition")]
    public Animator paperTransitionAnimator;

    [Header("Fake Main Menu Background")]
    [Tooltip("The Image containing the sky/clouds background")]
    public GameObject fakeMainMenuBackground;

    [Header("Timing")]
    public float boxExitDuration = 1f;
    public float paperAnimationDuration = 1f;

    // Call this from your "Yes" button's OnClick event!
    public void StartExitSequence(string sceneName)
    {
        StartCoroutine(ExitRoutine(sceneName));
    }

    private IEnumerator ExitRoutine(string sceneName)
    {
        // 1. Move the cardboard box away
        if (boxAnimator != null)
        {
            // Make sure your box animator has this exact trigger!
            boxAnimator.SetTrigger("BoxExit");
        }

        // Wait for the box to get off the screen
        yield return new WaitForSeconds(boxExitDuration);

        // 2. Turn on the fake Main Menu background so it's waiting behind the paper
        if (fakeMainMenuBackground != null)
        {
            fakeMainMenuBackground.SetActive(true);
        }

        // 3. Play the REVERSE paper animation (uncrumpling)
        if (paperTransitionAnimator != null)
        {
            // Ensure the paper is fully visible before playing
            Image paperImg = paperTransitionAnimator.GetComponent<Image>();
            if (paperImg != null)
            {
                Color c = paperImg.color;
                c.a = 1f;
                paperImg.color = c;
            }

            paperTransitionAnimator.SetTrigger("PaperReverseTrigger");
        }

        // Wait for the paper to finish uncrumpling and revealing the sky
        yield return new WaitForSeconds(paperAnimationDuration);

        // 4. Seamlessly load the real Main Menu scene right behind it!
        SceneManager.LoadScene(sceneName);
    }
}