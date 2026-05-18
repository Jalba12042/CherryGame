using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class WinScript : MonoBehaviour
{
    public static List<int> winningPlayers = new List<int>();

    [Header("UI Elements")]
    public TextMeshProUGUI winnerText;
    public string shopSceneName = "Shop";

    [Header("Scoreboard Flow")]
    public GameObject scoreboardClipboard;
    public ScoreboardUI scoreboardUI;

    [Header("Ready Up UI")]
    public GameObject pressAButtonPrompt;
    public Image[] playerReadyIcons;
    public GameObject[] playerReadyBackgrounds;
    public Sprite[] colorIcons;

    [Header("Name Mapping")]
    public string[] availableNames;

    [Header("Outro Animations")]
    public Animator clipboardAnimator;
    public Animator toShopTextAnimator;
    public Animator buttonAAnimator;
    public Animator[] headIconAnimators;
    public float waitBeforeNextScene = 1.5f;

    private bool[] playerReady = new bool[4];
    private int readyCount = 0;
    private bool isScoreboardVisible = false;

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

        // Set Winner Text
        if (isTie)
        {
            if (winnerText != null) winnerText.text = "IT'S A TIE!";
        }
        else
        {
            string winName = "PLAYER " + (winnerID + 1);
            if (GameManager.Instance != null && GameManager.Instance.playerCustomizations.Count > winnerID)
            {
                var data = GameManager.Instance.playerCustomizations[winnerID];
                if (availableNames != null && data.nameIndex >= 0 && data.nameIndex < availableNames.Length)
                {
                    winName = availableNames[data.nameIndex];
                }
            }
            if (winnerText != null) winnerText.text = winName + " WINS!";
        }

        // Show the Scoreboard
        if (scoreboardClipboard != null)
        {
            scoreboardClipboard.SetActive(true);
            if (scoreboardUI != null) scoreboardUI.UpdateScoreboard();
        }

        // Setup the Ready Up UI
        if (pressAButtonPrompt != null) pressAButtonPrompt.SetActive(true);

        int totalPlayers = GameManager.Instance != null ? GameManager.Instance.playerCount : 4;

        for (int i = 0; i < 4; i++)
        {
            bool isPlaying = i < totalPlayers;

            if (playerReadyBackgrounds != null && i < playerReadyBackgrounds.Length && playerReadyBackgrounds[i] != null)
            {
                playerReadyBackgrounds[i].SetActive(isPlaying);
            }

            if (playerReadyIcons != null && i < playerReadyIcons.Length && playerReadyIcons[i] != null)
            {
                playerReadyIcons[i].gameObject.SetActive(false);
            }
            playerReady[i] = false;
        }

        readyCount = 0;
        isScoreboardVisible = true;
    }

    private void Update()
    {
        if (!isScoreboardVisible || GameManager.Instance == null) return;

        int totalPlayers = GameManager.Instance.playerCount;

        for (int i = 0; i < totalPlayers; i++)
        {
            if (!playerReady[i] && InputManager.Instance.GetConfirmDown(i + 1))
                ReadyUpPlayer(i);
        }

        if (readyCount >= totalPlayers && totalPlayers > 0)
        {
            isScoreboardVisible = false;
            StartCoroutine(PlayOutroAndLoadShop());
        }
    }

    private void ReadyUpPlayer(int playerIndex)
    {
        playerReady[playerIndex] = true;
        readyCount++;

        if (playerReadyIcons[playerIndex] != null)
        {
            playerReadyIcons[playerIndex].gameObject.SetActive(true);

            if (GameManager.Instance.playerCustomizations.Count > playerIndex)
            {
                int colorIndex = GameManager.Instance.playerCustomizations[playerIndex].colorIndex;
                if (colorIndex >= 0 && colorIndex < colorIcons.Length)
                {
                    playerReadyIcons[playerIndex].sprite = colorIcons[colorIndex];
                }
            }
        }
    }

    private IEnumerator PlayOutroAndLoadShop()
    {
        // --- NEW: TELL THE MUSIC TO FADE OUT ---
        if (GameplayMusicManager.Instance != null)
        {
            GameplayMusicManager.Instance.FadeOutToShop(waitBeforeNextScene);
        }

        // 1. Instantly hide the "Ready" icons and backgrounds
        for (int i = 0; i < 4; i++)
        {
            if (playerReadyIcons != null && i < playerReadyIcons.Length && playerReadyIcons[i] != null)
            {
                playerReadyIcons[i].gameObject.SetActive(false);
            }

            if (playerReadyBackgrounds != null && i < playerReadyBackgrounds.Length && playerReadyBackgrounds[i] != null)
            {
                playerReadyBackgrounds[i].SetActive(false);
            }
        }

        // 2. Play Animators Outro
        if (clipboardAnimator != null) clipboardAnimator.SetTrigger("Outro");
        if (toShopTextAnimator != null) toShopTextAnimator.SetTrigger("Outro");
        if (buttonAAnimator != null) buttonAAnimator.SetTrigger("Outro");

        if (headIconAnimators != null)
        {
            foreach (Animator anim in headIconAnimators)
            {
                if (anim != null) anim.SetTrigger("Outro");
            }
        }

        // 3. Wait for animations and music fade
        yield return new WaitForSeconds(waitBeforeNextScene);

        // 4. Load Shop
        SceneManager.LoadScene(shopSceneName);
    }
}