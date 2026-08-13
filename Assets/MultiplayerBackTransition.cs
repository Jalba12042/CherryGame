using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MultiplayerBackTransition : MonoBehaviour
{
    public Animator eraserAnimator;
    public string eraserTrigger = "SlideWipe";
    public float wipeAnimationDuration = 1f;
    public string sceneToLoad = "Main Menu";

    // We will call this function from your buttons and your Menu Controller!
    public void PlayBackTransition()
    {
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
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