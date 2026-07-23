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
    public static PauseManager Instance { get; private set; }
    [Header("UI Canvases")]
    public GameObject pauseCanvas;
    public GameObject mainPlayerCanvas;

    [Header("Loading Screen Setup")]
    public GameObject loadingScreenPanel; // <--- NEW: Drag your loading Panel here!

    [Header("Pause Menu Elements")]
    public GameObject paperBackground;
    public GameObject buttonList;
    public GameObject playerInfoBox;
    public TMP_Text countdownText;
    public GameObject buttonPrompts;

    [Header("Controls Screen Elements")]
    public GameObject controlsPanel;
    public Image controlsImageDisplay;
    public Sprite[] controlsPages;

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
    private bool isShowingControls = false;
    private int currentControlPage = 0;

    public bool IsPaused => isPaused || isCountingDown;

    private Gamepad controllingGamepad;
    private int currentIndex = 0;
    private bool canMove = true;
    private float deadzone = 0.5f;

    void Start()
    {
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (mainPlayerCanvas != null) mainPlayerCanvas.SetActive(true);
    }

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (isCountingDown) return;

        if (!isPaused)
        {
            // --- NEW CODE: STOP PAUSING IF THE LOADING SCREEN IS ACTIVE ---
            if (loadingScreenPanel != null && loadingScreenPanel.activeInHierarchy)
            {
                return; // Exits the update loop immediately so the Start button does nothing
            }
            // --------------------------------------------------------------

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

            // --- CONTROLS SCREEN LOGIC ---
            if (isShowingControls)
            {
                if (controllingGamepad.buttonEast.wasPressedThisFrame)
                {
                    CloseControls();
                    return;
                }

                Vector2 move = controllingGamepad.leftStick.ReadValue();

                if (canMove)
                {
                    if (move.x > deadzone || controllingGamepad.dpad.right.wasPressedThisFrame)
                    {
                        ChangeControlPage(1);
                        canMove = false;
                    }
                    else if (move.x < -deadzone || controllingGamepad.dpad.left.wasPressedThisFrame)
                    {
                        ChangeControlPage(-1);
                        canMove = false;
                    }
                }

                if (Mathf.Abs(move.x) < 0.2f && !controllingGamepad.dpad.left.isPressed && !controllingGamepad.dpad.right.isPressed)
                {
                    canMove = true;
                }

                return;
            }

            // --- NORMAL PAUSE MENU LOGIC ---
            if (controllingGamepad.startButton.wasPressedThisFrame || controllingGamepad.buttonEast.wasPressedThisFrame)
            {
                StartResumeSequence();
                return;
            }

            Vector2 menuMove = controllingGamepad.leftStick.ReadValue();

            if (canMove)
            {
                if (menuMove.y > deadzone || controllingGamepad.dpad.up.wasPressedThisFrame)
                {
                    currentIndex = Mathf.Max(0, currentIndex - 1);
                    HighlightButton();
                    canMove = false;
                }
                else if (menuMove.y < -deadzone || controllingGamepad.dpad.down.wasPressedThisFrame)
                {
                    currentIndex = Mathf.Min(menuButtons.Length - 1, currentIndex + 1);
                    HighlightButton();
                    canMove = false;
                }
            }

            if (Mathf.Abs(menuMove.y) < 0.2f && !controllingGamepad.dpad.up.isPressed && !controllingGamepad.dpad.down.isPressed)
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
                OpenControls();
                break;
            case 3:
                QuitToMainMenu();
                break;
        }
    }

    private void OpenControls()
    {
        isShowingControls = true;
        currentControlPage = 0;

        if (paperBackground != null) paperBackground.SetActive(false);
        if (buttonList != null) buttonList.SetActive(false);
        if (playerInfoBox != null) playerInfoBox.SetActive(false);

        if (controlsPanel != null) controlsPanel.SetActive(true);
        UpdateControlsImage();
    }

    private void CloseControls()
    {
        isShowingControls = false;

        if (controlsPanel != null) controlsPanel.SetActive(false);

        if (paperBackground != null) paperBackground.SetActive(true);
        if (buttonList != null) buttonList.SetActive(true);
        if (playerInfoBox != null) playerInfoBox.SetActive(true);
    }

    private void ChangeControlPage(int direction)
    {
        if (controlsPages == null || controlsPages.Length == 0) return;

        currentControlPage += direction;

        if (currentControlPage >= controlsPages.Length) currentControlPage = 0;
        else if (currentControlPage < 0) currentControlPage = controlsPages.Length - 1;

        UpdateControlsImage();
    }

    private void UpdateControlsImage()
    {
        if (controlsImageDisplay != null && controlsPages != null && controlsPages.Length > 0)
        {
            controlsImageDisplay.sprite = controlsPages[currentControlPage];
        }
    }

    public void PauseGame(Gamepad pad, int padIndex)
    {
        isPaused = true;
        isShowingControls = false;
        controllingGamepad = pad;

        // I uncommented this so your game UI hides properly when paused during a match!
        if (mainPlayerCanvas != null) mainPlayerCanvas.SetActive(false);

        if (pauseCanvas != null) pauseCanvas.SetActive(true);

        if (paperBackground != null) paperBackground.SetActive(true);
        if (buttonList != null) buttonList.SetActive(true);
        if (playerInfoBox != null) playerInfoBox.SetActive(true);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);

        if (buttonPrompts != null) buttonPrompts.SetActive(true);

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
        if (buttonPrompts != null) buttonPrompts.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);

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

    private void RestartMatch()
    {
        Time.timeScale = 1f;

        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.StopAllRoundLogic();
            RoundManager.Instance.currRoundProgress = 0f;
        }

        if (EventManager.Instance != null)
        {
            EventManager.Instance.SoftReset();
        }

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