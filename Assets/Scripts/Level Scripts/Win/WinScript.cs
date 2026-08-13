using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class WinScript : MonoBehaviour
{
    public static List<int> winningPlayers = new List<int>();

    [Header("Scene Settings")]
    public string shopSceneName = "Shop";
    public float autoTransitionDelay = 8f; // <-- The 8-second Gang Beasts style wait!

    [Header("Scoreboard Flow")]
    public GameObject scoreboardClipboard;
    public ScoreboardUI scoreboardUI;

    [Header("Name Mapping")]
    public string[] availableNames;
    public Sprite[] colorIcons;

    [Header("Outro Animations")]
    public Animator clipboardAnimator;
    public Animator[] headIconAnimators;
    public float waitBeforeNextScene = 1.5f;

    void Start()
    {
        // Snap time back to 100% normal speed!
        Time.timeScale = 1f;

        bool isTie = winningPlayers.Count > 1;
        int winnerID = winningPlayers.Count > 0 ? winningPlayers[0] : 0;

        // Add the point to the GameManager
        if (!isTie && winningPlayers.Count > 0 && GameManager.Instance != null)
        {
            if (winnerID < GameManager.Instance.playerTotalScores.Length)
            {
                GameManager.Instance.playerTotalScores[winnerID]++;
            }
        }

        // Show the Scoreboard
        if (scoreboardClipboard != null)
        {
            scoreboardClipboard.SetActive(true);
            if (scoreboardUI != null) scoreboardUI.UpdateScoreboard();
        }

        // Start the auto-transition timer immediately!
        StartCoroutine(AutoTransitionTimer());
    }

    private IEnumerator AutoTransitionTimer()
    {
        // Wait for 8 seconds (let players trash talk and look at the scores)
        yield return new WaitForSeconds(autoTransitionDelay);

        // Trigger the outro and load the shop
        StartCoroutine(PlayOutroAndLoadShop());
    }

    private IEnumerator PlayOutroAndLoadShop()
    {
        // --- TELL THE MUSIC TO FADE OUT ---
        if (GameplayMusicManager.Instance != null)
        {
            GameplayMusicManager.Instance.FadeOutToShop(waitBeforeNextScene);
        }

        // Play Animators Outro
        if (clipboardAnimator != null) clipboardAnimator.SetTrigger("Outro");

        if (headIconAnimators != null)
        {
            foreach (Animator anim in headIconAnimators)
            {
                if (anim != null) anim.SetTrigger("Outro");
            }
        }

        // Wait for animations and music fade
        yield return new WaitForSeconds(waitBeforeNextScene);

        // Load Shop
        SceneManager.LoadScene(shopSceneName);
    }
}