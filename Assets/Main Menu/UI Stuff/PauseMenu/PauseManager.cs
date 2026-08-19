using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
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
    public GameObject loadingScreenPanel;

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

    [Header("Dynamic Button Prompts (Images)")]
    public Image selectButtonImage;
    public Image backButtonImage;

    [Header("Xbox Prompts")]
    public Sprite xboxSelectSprite;
    public Sprite xboxBackSprite;

    [Header("PlayStation Prompts")]
    public Sprite psSelectSprite;
    public Sprite psBackSprite;

    [Header("Keyboard Prompts")]
    public Sprite kbSelectSprite;
    public Sprite kbBackSprite;

    private bool isPaused = false;
    private bool isCountingDown = false;
    private bool isShowingControls = false;
    private int currentControlPage = 0;

    public bool IsPaused => isPaused || isCountingDown;

    private Gamepad controllingGamepad;
    private bool pausedByKeyboard = false;

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
            if (loadingScreenPanel != null && loadingScreenPanel.activeInHierarchy)
            {
                return;
            }

            if (RoundManager.Instance != null && RoundManager.Instance.currRound != null)
            {
                if (SceneManager.GetActiveScene().name == RoundManager.Instance.currRound.sceneName && !RoundManager.Instance.currRoundActive)
                {
                    return;
                }
            }

            if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame))
            {
                PauseGame(null, 99, true);
                return;
            }

            var allPads = Gamepad.all;
            for (int i = 0; i < allPads.Count; i++)
            {
                if (allPads[i].startButton.wasPressedThisFrame)
                {
                    PauseGame(allPads[i], i, false);
                    break;
                }
            }
        }
        else
        {
            if (controllingGamepad == null && !pausedByKeyboard) return;

            bool confirmPressed = false;
            bool backPressed = false;
            Vector2 move = Vector2.zero;

            if (pausedByKeyboard && Keyboard.current != null)
            {
                if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame) confirmPressed = true;
                if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.backspaceKey.wasPressedThisFrame) backPressed = true;

                if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame) move.y = 1f;
                if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame) move.y = -1f;
                if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame) move.x = 1f;
                if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame) move.x = -1f;
            }
            else if (controllingGamepad != null)
            {
                confirmPressed = controllingGamepad.buttonSouth.wasPressedThisFrame;
                backPressed = controllingGamepad.buttonEast.wasPressedThisFrame;

                move = controllingGamepad.leftStick.ReadValue();
                if (controllingGamepad.dpad.up.wasPressedThisFrame) move.y = 1f;
                if (controllingGamepad.dpad.down.wasPressedThisFrame) move.y = -1f;
                if (controllingGamepad.dpad.right.wasPressedThisFrame) move.x = 1f;
                if (controllingGamepad.dpad.left.wasPressedThisFrame) move.x = -1f;

                if (controllingGamepad.startButton.wasPressedThisFrame) backPressed = true;
            }

            if (isShowingControls)
            {
                if (backPressed)
                {
                    CloseControls();
                    return;
                }

                if (canMove)
                {
                    if (move.x > deadzone)
                    {
                        ChangeControlPage(1);
                        canMove = false;
                    }
                    else if (move.x < -deadzone)
                    {
                        ChangeControlPage(-1);
                        canMove = false;
                    }
                }

                if (Mathf.Abs(move.x) < 0.2f)
                {
                    canMove = true;
                }

                return;
            }

            if (backPressed)
            {
                StartResumeSequence();
                return;
            }

            if (canMove)
            {
                if (move.y > deadzone)
                {
                    currentIndex = Mathf.Max(0, currentIndex - 1);
                    HighlightButton();
                    canMove = false;
                }
                else if (move.y < -deadzone)
                {
                    currentIndex = Mathf.Min(menuButtons.Length - 1, currentIndex + 1);
                    HighlightButton();
                    canMove = false;
                }
            }

            if (Mathf.Abs(move.y) < 0.2f)
            {
                canMove = true;
            }

            if (confirmPressed)
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
                OpenControls();
                break;
            case 2:
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

    public void PauseGame(Gamepad pad, int padIndex, bool isKeyboard = false)
    {
        isPaused = true;
        isShowingControls = false;
        controllingGamepad = pad;
        pausedByKeyboard = isKeyboard;

        if (mainPlayerCanvas != null) mainPlayerCanvas.SetActive(false);
        if (pauseCanvas != null) pauseCanvas.SetActive(true);

        if (paperBackground != null) paperBackground.SetActive(true);
        if (buttonList != null) buttonList.SetActive(true);
        if (playerInfoBox != null) playerInfoBox.SetActive(true);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);

        if (buttonPrompts != null) buttonPrompts.SetActive(true);

        UpdatePlayerInfoUI(padIndex);
        UpdateDynamicPrompts();

        // --- NEW: Flag the background music to ignore the upcoming pause freeze! ---
        if (GameplayMusicManager.Instance != null)
        {
            AudioSource bgmSource = GameplayMusicManager.Instance.GetComponentInChildren<AudioSource>();
            if (bgmSource != null)
            {
                bgmSource.ignoreListenerPause = true;
            }
        }

        // Stop all sound effects instantly (except the music we just flagged)
        AudioListener.pause = true;
        Time.timeScale = 0f;

        currentIndex = 0;
        HighlightButton();
    }

    private void UpdateDynamicPrompts()
    {
        if (selectButtonImage == null || backButtonImage == null) return;

        if (pausedByKeyboard)
        {
            selectButtonImage.sprite = kbSelectSprite;
            backButtonImage.sprite = kbBackSprite;
        }
        else if (controllingGamepad is DualShockGamepad)
        {
            selectButtonImage.sprite = psSelectSprite;
            backButtonImage.sprite = psBackSprite;
        }
        else
        {
            selectButtonImage.sprite = xboxSelectSprite;
            backButtonImage.sprite = xboxBackSprite;
        }
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
        AudioListener.pause = false;

        isPaused = false;
        isCountingDown = false;
        controllingGamepad = null;
        pausedByKeyboard = false;
    }

    private void QuitToMainMenu()
    {
        // --- NEW: Route the quit button into our custom fade coroutine! ---
        StartCoroutine(QuitCoroutine());
    }

    private IEnumerator QuitCoroutine()
    {
        float elapsed = 0f;
        float fadeDuration = 1.0f;

        AudioSource bgmSource = null;
        float startVol = 1f;

        if (GameplayMusicManager.Instance != null)
        {
            bgmSource = GameplayMusicManager.Instance.GetComponentInChildren<AudioSource>();
            if (bgmSource != null) startVol = bgmSource.volume;
        }

        // Fade out the music using unscaledDeltaTime so it still works while the game is frozen!
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (bgmSource != null)
            {
                bgmSource.volume = Mathf.Lerp(startVol, 0f, elapsed / fadeDuration);
            }
            yield return null;
        }

        // Now that the fade is done, unfreeze everything and load the menu
        Time.timeScale = 1f;
        AudioListener.pause = false;

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