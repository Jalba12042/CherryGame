using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuTransition : MonoBehaviour
{
    [Header("Menu Exit Setup")]
    [Tooltip("Drag all 13 of your menu animators (Hill, Rain, Buttons, etc.) here")]
    public Animator[] exitAnimators;

    [Tooltip("The trigger name used to make them leave")]
    public string exitTriggerName = "Exit";

    [Header("Paper Transition Setup")]
    [Tooltip("The Animator on your PaperTransitionDrop object")]
    public Animator paperTransitionAnimator;

    [Header("Timing")]
    [Tooltip("How long to wait after the menu slides down before the paper opens")]
    public float delayBeforePaperOpens = 1.14f;

    [Tooltip("How long the paper animation takes to play before loading the scene")]
    public float paperAnimationDuration = 1f;

    public void LoadSceneWithPaperWipe(string sceneName)
    {
        StartCoroutine(TransitionSequence(sceneName));
    }

    private IEnumerator TransitionSequence(string sceneName)
    {
        // 1. Trigger all menu elements (hill, buttons, clouds) to slide away
        foreach (Animator anim in exitAnimators)
        {
            if (anim != null)
            {
                anim.SetTrigger(exitTriggerName);
            }
        }

        // 2. Wait for the menu elements to finish leaving (1.14 seconds)
        yield return new WaitForSeconds(delayBeforePaperOpens);

        // 3. Trigger the paper video wipe animation
        if (paperTransitionAnimator != null)
        {
            // Snap the alpha back to 1 instantly before playing the animation
            Image paperImg = paperTransitionAnimator.GetComponent<Image>();
            if (paperImg != null)
            {
                Color c = paperImg.color;
                c.a = 1f;
                paperImg.color = c;
            }

            paperTransitionAnimator.SetTrigger("PaperOpen");
        }

        // 4. Wait for the paper to finish crumpling/opening
        yield return new WaitForSeconds(paperAnimationDuration);

        // 5. Load the next scene seamlessly behind it!
        SceneManager.LoadScene(sceneName);
    }
}