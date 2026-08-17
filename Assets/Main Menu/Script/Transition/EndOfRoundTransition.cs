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
    [Tooltip("Drag your Ambience_Manager here so it fades out at the end!")]
    public AudioSource levelAmbience;

    [Header("Transition Audio")]
    [Tooltip("Drag the Audio Source for the Paper Transition sound here!")]
    public AudioSource paperSlideAudio; // <-- NEW AUDIO SOURCE SLOT

    [Header("Transition Settings")]
    public string nextSceneName;
    [Tooltip("How long the paper takes to slide down (in seconds)")]
    public float paperSlideSpeed = 10f;

    public void TriggerEndSequence()
    {
        // Tell the music manager to fade out both the song and the ambience!
        if (GameplayMusicManager.Instance != null)
        {
            GameplayMusicManager.Instance.StopMusicAndAmbience(levelAmbience);
        }

        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        // 1. Play the whistle sound
        if (whistleAudio != null) whistleAudio.Play();

        // 2. Make the text visible AND change what it says!
        if (timeUpTxt != null)
        {
            timeUpTxt.text = "Game!";
            timeUpTxt.gameObject.SetActive(true);
        }

        // Wait 2 seconds so players can react to the timer ending
        yield return new WaitForSeconds(2f);

        // 3. Make the paper visible, play the sound, and prepare it to slide
        if (paperWipeUI != null)
        {
            paperWipeUI.gameObject.SetActive(true);

            // --- NEW: Play the transition sound! ---
            if (paperSlideAudio != null) paperSlideAudio.Play();

            Vector2 startPosition = new Vector2(0, Screen.height * 2);
            Vector2 targetPosition = Vector2.zero;

            paperWipeUI.anchoredPosition = startPosition;

            float elapsedTime = 0f;

            // 4. Slide the paper over exactly 10 seconds
            while (elapsedTime < paperSlideSpeed)
            {
                elapsedTime += Time.deltaTime;
                float transitionProgress = elapsedTime / paperSlideSpeed;

                paperWipeUI.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, transitionProgress);

                yield return null;
            }

            paperWipeUI.anchoredPosition = targetPosition;
        }

        // 5. Load the ScoreScene
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadSceneAsync(nextSceneName);
        }
    }
}