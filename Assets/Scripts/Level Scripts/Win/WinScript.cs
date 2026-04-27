using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
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
    public Image[] playerReadyIcons;          // The colored faces
    public GameObject[] playerReadyBackgrounds; // NEW: Drag your 4 White Boxes here!
    public Sprite[] colorIcons;

    [Header("Name Mapping")]
    public string[] availableNames;

    private bool[] playerReady = new bool[4];
    private int readyCount = 0;
    private bool isScoreboardVisible = false;

    void Start()
    {
        bool isTie = winningPlayers.Count > 1;
        int winnerID = winningPlayers.Count > 0 ? winningPlayers[0] : 0;

        if (!isTie && winningPlayers.Count > 0 && GameManager.Instance != null)
        {
            if (winnerID < GameManager.Instance.playerTotalScores.Length)
            {
                GameManager.Instance.playerTotalScores[winnerID]++;
            }
        }

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

        // Setup the Ready Up UI & Hide extra white boxes!
        if (pressAButtonPrompt != null) pressAButtonPrompt.SetActive(true);

        int totalPlayers = GameManager.Instance != null ? GameManager.Instance.playerCount : 4;

        for (int i = 0; i < 4; i++)
        {
            bool isPlaying = i < totalPlayers;

            // Hides the White Box if they aren't playing!
            if (playerReadyBackgrounds != null && i < playerReadyBackgrounds.Length && playerReadyBackgrounds[i] != null)
            {
                playerReadyBackgrounds[i].SetActive(isPlaying);
            }

            // Keep the face hidden until they press A
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

        if (GameManager.Instance.isOnKeyboard)
        {
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            {
                if (!playerReady[0]) ReadyUpPlayer(0);
            }
        }
        else
        {
            var controllers = Gamepad.all.ToArray();
            for (int c = 0; c < controllers.Length; c++)
            {
                Gamepad pad = controllers[c];
                if (pad == null) continue;

                if (pad.buttonSouth.wasPressedThisFrame)
                {
                    for (int i = 0; i < totalPlayers; i++)
                    {
                        if (GameManager.Instance.controllerAssignments[i] == c)
                        {
                            if (!playerReady[i]) ReadyUpPlayer(i);
                        }
                    }
                }
            }
        }

        if (readyCount >= totalPlayers && totalPlayers > 0)
        {
            isScoreboardVisible = false;
            SceneManager.LoadScene(shopSceneName);
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
}