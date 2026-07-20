using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndOfRoundTransition : MonoBehaviour
{
    [Header("UI & Audio References")]
    public TextMeshProUGUI timeUpTxt;
    public AudioSource whistleAudio;
    public RectTransform paperWipeUI;

    [Header("Transition Settings")]
    public string nextSceneName;
    [Tooltip("How long the paper takes to slide down (in seconds)")]
    public float paperSlideSpeed = 10f; // Set to 10 seconds per your request!

    public void TriggerEndSequence()
    {
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        // 1. Play the whistle sound
        if (whistleAudio != null) whistleAudio.Play();

        // 2. Make the "Time's Up" text visible!
        if (timeUpTxt != null)
        {
            timeUpTxt.gameObject.SetActive(true);
        }

        // Wait 2 seconds so players can react to the timer ending
        yield return new WaitForSeconds(2f);

        // 3. Make the paper visible and prepare it to slide
        if (paperWipeUI != null)
        {
            paperWipeUI.gameObject.SetActive(true);

            // Start the paper high above the screen
            Vector2 startPosition = new Vector2(0, Screen.height * 2);
            // End the paper exactly in the middle of the screen
            Vector2 targetPosition = Vector2.zero;

            paperWipeUI.anchoredPosition = startPosition;

            float elapsedTime = 0f;

            // 4. Slide the paper over exactly 10 seconds!
            while (elapsedTime < paperSlideSpeed)
            {
                elapsedTime += Time.deltaTime;
                float transitionProgress = elapsedTime / paperSlideSpeed;

                // Smoothly slide from the top down to the center
                paperWipeUI.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, transitionProgress);

                yield return null;
            }

            // Ensure it is perfectly centered at the end of the 10 seconds
            paperWipeUI.anchoredPosition = targetPosition;
        }

        // 5. Load the ScoreScene (as set in your Inspector)
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadSceneAsync(nextSceneName);
        }
    }
}