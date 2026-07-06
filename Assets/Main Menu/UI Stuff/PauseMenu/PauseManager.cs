using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class CustomPauseButton
{
    public Image buttonImage;
    public Sprite normalSprite;
    public Sprite highlightSprite;
}

public class PauseManager : MonoBehaviour
{
    [Header("UI Canvases")]
    public GameObject pauseCanvas;
    public GameObject mainPlayerCanvas;

    [Header("Pause Menu Elements")]
    public GameObject paperBackground;
    public GameObject buttonList;
    public GameObject playerInfoBox;
    public TMP_Text countdownText;

    [Header("Image Buttons")]
    public CustomPauseButton[] menuButtons;

    [Header("Player Info Maps")]
    public Image pausingPlayerIcon;
    public TMP_Text pausingPlayerText;
    public string[] availableNames;
    public Sprite[] colorIcons;

    [Header("Scene Settings")]
    public string titleSceneName = "Main Menu";

    private bool isPaused = false;
    private bool isCountingDown = false;
    private Gamepad controllingGamepad;
    private int currentIndex = 0;
    private bool canMove = true;
    private float deadzone = 0.5f;

    void Start()
    {
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (mainPlayerCanvas != null) mainPlayerCanvas.SetActive(true);
    }

    void Update()
    {
        if (isCountingDown) return;

        if (!isPaused)
        {
            var allPads = Gamepad.all;
            for (int i = 0; i < allPads.Count; i++)
            {
                if (allPads[i].startButton.wasPressedThisFrame)
                {
                    PauseGame(allPads[i], i);
                    break;
                }
            }
        }
        else
        {
            if (controllingGamepad == null) return;

            if (controllingGamepad.startButton.wasPressedThisFrame || controllingGamepad.buttonEast.wasPressedThisFrame)
            {
                StartResumeSequence();
                return;
            }

            Vector2 move = controllingGamepad.leftStick.ReadValue();

            if (canMove)
            {
                if (move.y > deadzone || controllingGamepad.dpad.up.wasPressedThisFrame)
                {
                    currentIndex = Mathf.Max(0, currentIndex - 1);
                    HighlightButton();
                    canMove = false;
                }
                else if (move.y < -deadzone || controllingGamepad.dpad.down.wasPressedThisFrame)
                {
                    currentIndex = Mathf.Min(menuButtons.Length - 1, currentIndex + 1);
                    HighlightButton();
                    canMove = false;
                }
            }

            if (Mathf.Abs(move.y) < 0.2f && !controllingGamepad.dpad.up.isPressed && !controllingGamepad.dpad.down.isPressed)
            {
                canMove = true;
            }

            if (controllingGamepad.buttonSouth.wasPressedThisFrame)
            {
                ExecuteMenuAction(currentIndex);
            }
        }
    }

    private void ExecuteMenuAction(int index)
    {
        switch (index)
        {
            case 0:
                StartResumeSequence();
                break;
            case 1:
                RestartMatch();
                break;
            case 2:
                QuitToMainMenu();
                break;
        }
    }

    public void PauseGame(Gamepad pad, int padIndex)
    {
        isPaused = true;
        controllingGamepad = pad;

        if (mainPlayerCanvas != null) mainPlayerCanvas.SetActive(false);
        if (pauseCanvas != null) pauseCanvas.SetActive(true);

        if (paperBackground != null) paperBackground.SetActive(true);
        if (buttonList != null) buttonList.SetActive(true);
        if (playerInfoBox != null) playerInfoBox.SetActive(true);
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        UpdatePlayerInfoUI(padIndex);

        Time.timeScale = 0f;

        currentIndex = 0;
        HighlightButton();
    }

    private void UpdatePlayerInfoUI(int padIndex)
    {
        int playerIndex = padIndex;

        if (GameManager.Instance != null && GameManager.Instance.controllerAssignments != null)
        {
            for (int i = 0; i < GameManager.Instance.controllerAssignments.Length; i++)
            {
                if (GameManager.Instance.controllerAssignments[i] == padIndex)
                {
                    playerIndex = i;
                    break;
                }
            }
        }

        if (playerIndex == -1) playerIndex = 0;

        string pName = "PLAYER " + (playerIndex + 1);
        Sprite pIcon = null;

        if (GameManager.Instance != null && playerIndex < GameManager.Instance.playerCustomizations.Count)
        {
            var data = GameManager.Instance.playerCustomizations[playerIndex];
            if (availableNames != null && data.nameIndex >= 0 && data.nameIndex < availableNames.Length)
                pName = availableNames[data.nameIndex];

            if (colorIcons != null && data.colorIndex >= 0 && data.colorIndex < colorIcons.Length)
                pIcon = colorIcons[data.colorIndex];
        }

        if (pausingPlayerText != null) pausingPlayerText.text = pName.ToUpper() + " PAUSED";
        if (pausingPlayerIcon != null)
        {
            pausingPlayerIcon.gameObject.SetActive(pIcon != null);
            if (pIcon != null) pausingPlayerIcon.sprite = pIcon;
        }
    }

    private void StartResumeSequence()
    {
        StartCoroutine(ResumeCoroutine());
    }

    private IEnumerator ResumeCoroutine()
    {
        isCountingDown = true;

        if (buttonList != null) buttonList.SetActive(false);
        if (playerInfoBox != null) playerInfoBox.SetActive(false);
        if (paperBackground != null) paperBackground.SetActive(false);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "3";
            yield return new WaitForSecondsRealtime(1f);
            countdownText.text = "2";
            yield return new WaitForSecondsRealtime(1f);
            countdownText.text = "1";
            yield return new WaitForSecondsRealtime(1f);
            countdownText.text = "GO!";
            yield return new WaitForSecondsRealtime(0.5f);
            countdownText.gameObject.SetActive(false);
        }

        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        if (mainPlayerCanvas != null) mainPlayerCanvas.SetActive(true);

        Time.timeScale = 1f;
        isPaused = false;
        isCountingDown = false;
        controllingGamepad = null;
    }

    // ==========================================
    // --- THE PERFECT SCORE-SAVING RESTART ---
    // ==========================================
    private void RestartMatch()
    {
        // 1. Unfreeze the game!
        Time.timeScale = 1f;

        // 2. Safely stop the background timers without wiping the score memory!
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.StopAllRoundLogic();
            RoundManager.Instance.currRoundProgress = 0f; // Reset progress for the fresh start
            // Notice we do NOT touch roundsWon or currRound! It remembers the score and the map.
        }

        // 3. Reset the event UI so animations don't get stuck on screen
        if (EventManager.Instance != null)
        {
            EventManager.Instance.SoftReset();
        }

        // 4. Reload the exact same map scene!
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void QuitToMainMenu()
    {
        Time.timeScale = 1f;

        if (GameplayMusicManager.Instance != null) Destroy(GameplayMusicManager.Instance.gameObject);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.HardResetForMainMenu();
        }

        SceneManager.LoadScene(titleSceneName);
    }

    private void HighlightButton()
    {
        if (menuButtons == null) return;
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i].buttonImage != null)
                menuButtons[i].buttonImage.sprite = (i == currentIndex) ? menuButtons[i].highlightSprite : menuButtons[i].normalSprite;
        }
    }
}