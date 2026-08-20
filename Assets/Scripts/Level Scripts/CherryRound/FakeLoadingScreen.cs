using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock; // <-- Added to detect PlayStation controllers

public class FakeLoadingScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject loadingPanel;
    public Image loadingBarFill;
    public GameObject pressAButtonPrompt;
    public TMP_Text tipsText;
    public float tipInterval = 2f;
    public TMP_Text levelTitleText;

    // ==========================================
    // --- NEW: DEVICE-SPECIFIC TIPS ---
    // ==========================================
    [Header("Device Specific Tips")]
    public string[] xboxTips;
    public string[] psTips;
    public string[] kbTips;

    private string[] currentActiveTips;
    private string mapSpecificTip = "";

    private enum DeviceMode { Xbox, PlayStation, Keyboard }
    private DeviceMode currentDeviceMode = DeviceMode.Xbox; // Default

    [Header("--- THEATER CURTAINS ---")]
    public Animator curtainAnimator;

    [Header("Ready Up UI")]
    public Image[] playerReadyIcons;
    private bool[] playerReady = new bool[4];
    private int totalPlayers;
    private int readyCount;
    public TimerUIManager timerManager;

    [Header("Pac-Man Loading Elements")]
    public Image playerIcon;
    public RectTransform startPoint;
    public RectTransform endPoint;
    public GameObject[] cherries;

    [Header("Puppet Show Groups (Parents)")]
    public GameObject parkShow;
    public GameObject beachShow;
    public GameObject mountainShow;

    [Header("Park Theme")]
    public Sprite parkCherrySprite;
    public Sprite runningSprite;
    public Sprite sittingSprite;
    public Vector3 parkItemScale = new Vector3(1f, 1f, 1f);
    public Vector3 parkRunScale = new Vector3(1f, 1f, 1f);
    public Vector3 parkSitScale = new Vector3(0.6f, 0.6f, 1f);

    [Header("Beach Theme")]
    public Sprite beachShellSprite;
    public Sprite beachRunningSprite;
    public Sprite beachSittingSprite;
    public Vector3 beachItemScale = new Vector3(1f, 1f, 1f);
    public Vector3 beachRunScale = new Vector3(1f, 1f, 1f);
    public Vector3 beachSitScale = new Vector3(0.6f, 0.6f, 1f);

    [Header("Mountain Theme")]
    public Sprite mountainSnowballSprite;
    public Sprite mountainRunningSprite;
    public Sprite mountainSittingSprite;
    public Vector3 mountainItemScale = new Vector3(1f, 1f, 1f);
    public Vector3 mountainRunScale = new Vector3(1f, 1f, 1f);
    public Vector3 mountainSitScale = new Vector3(0.6f, 0.6f, 1f);

    private Sprite activeRunSprite;
    private Sprite activeSitSprite;
    private Vector3 activeRunScale;
    private Vector3 activeSitScale;

    [Header("Timing")]
    public float loadDuration = 10f;
    [Tooltip("How long the player waits at the Start Point before running")]
    public float startDelay = 1.0f;
    private float currentDelay = 0f;
    private float timer = 0f;
    private bool loadingComplete = false;
    private float tipTimer = 0f;
    private int currentTip = 0;
    private bool roundStarted = false;

    [Header("Color Icons")]
    public Sprite[] colorIcons;

    [Header("Audio Polish")]
    public AudioSource levelAmbience;

    private void Start()
    {
        if (curtainAnimator != null)
        {
            curtainAnimator.SetTrigger("OpenCurtains");
        }

        ApplyMapTheme();

        // Force an initial build of the tips array based on Xbox default
        SwitchDeviceMode(DeviceMode.Xbox);

        foreach (var player in FindObjectsByType<Playermovement>(FindObjectsSortMode.None))
        {
            player.allowJumpInput = false;
        }

        loadingPanel.SetActive(true);
        pressAButtonPrompt.SetActive(false);
        if (loadingBarFill != null) loadingBarFill.fillAmount = 0;

        timer = 0f;
        currentDelay = 0f;
        loadingComplete = false;

        if (playerIcon != null && activeRunSprite != null)
        {
            playerIcon.sprite = activeRunSprite;
            playerIcon.rectTransform.localScale = activeRunScale;
            if (startPoint != null) playerIcon.rectTransform.position = startPoint.position;
        }

        if (cherries != null)
        {
            foreach (GameObject cherry in cherries)
            {
                if (cherry != null) cherry.SetActive(true);
            }
        }

        if (GameManager.Instance != null)
            totalPlayers = GameManager.Instance.playerCount;
        else
            totalPlayers = 0;

        for (int i = 0; i < playerReadyIcons.Length; i++)
        {
            if (playerReadyIcons[i] != null)
            {
                playerReadyIcons[i].gameObject.SetActive(false);
            }
            playerReady[i] = false;
        }

        readyCount = 0;
    }

    private void ApplyMapTheme()
    {
        if (parkShow != null) parkShow.SetActive(false);
        if (beachShow != null) beachShow.SetActive(false);
        if (mountainShow != null) mountainShow.SetActive(false);

        string sceneName = SceneManager.GetActiveScene().name;

        Sprite currentItemSprite = parkCherrySprite;
        Vector3 currentItemScale = parkItemScale;

        activeRunSprite = runningSprite;
        activeSitSprite = sittingSprite;
        activeRunScale = parkRunScale;
        activeSitScale = parkSitScale;

        if (sceneName.Contains("Beach"))
        {
            if (levelTitleText != null) levelTitleText.text = "Level Loading: Beach";
            currentItemSprite = beachShellSprite;
            currentItemScale = beachItemScale;

            activeRunSprite = beachRunningSprite;
            activeSitSprite = beachSittingSprite;
            activeRunScale = beachRunScale;
            activeSitScale = beachSitScale;

            if (beachShow != null) beachShow.SetActive(true);
            mapSpecificTip = "Tip: High tides will drag you into the water!"; // Replaced array override with variable assignment
        }
        else if (sceneName.Contains("Mountain") || sceneName.Contains("Iceberg"))
        {
            if (levelTitleText != null) levelTitleText.text = "Level Loading: Mountain";
            currentItemSprite = mountainSnowballSprite;
            currentItemScale = mountainItemScale;

            activeRunSprite = mountainRunningSprite;
            activeSitSprite = mountainSittingSprite;
            activeRunScale = mountainRunScale;
            activeSitScale = mountainSitScale;

            if (mountainShow != null) mountainShow.SetActive(true);
            mapSpecificTip = "Did you know? Snowballs deal extra knockback!"; // Replaced array override with variable assignment
        }
        else
        {
            if (levelTitleText != null) levelTitleText.text = "Level Loading: Park";

            if (parkShow != null) parkShow.SetActive(true);
            mapSpecificTip = "Watch out for the meteor!"; // Replaced array override with variable assignment
        }

        if (cherries != null)
        {
            foreach (GameObject item in cherries)
            {
                if (item != null)
                {
                    Image img = item.GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite = currentItemSprite;
                        img.rectTransform.localScale = currentItemScale;
                    }
                }
            }
        }
    }

    // --- NEW: Function to dynamically swap tips ---
    private void SwitchDeviceMode(DeviceMode newMode)
    {
        currentDeviceMode = newMode;
        List<string> compiledTips = new List<string>();

        // Always add the map tip first if we have one
        if (!string.IsNullOrEmpty(mapSpecificTip))
        {
            compiledTips.Add(mapSpecificTip);
        }

        // Append the correct controller tips
        string[] deviceTips = xboxTips;
        if (newMode == DeviceMode.PlayStation) deviceTips = psTips;
        else if (newMode == DeviceMode.Keyboard) deviceTips = kbTips;

        if (deviceTips != null)
        {
            compiledTips.AddRange(deviceTips);
        }

        currentActiveTips = compiledTips.ToArray();
        currentTip = 0;
        tipTimer = 0f; // Reset timer so they have time to read the new tip

        if (currentActiveTips.Length > 0 && tipsText != null)
        {
            tipsText.text = currentActiveTips[0];
        }
    }

    private void Update()
    {
        // ==========================================
        // --- NEW: DYNAMIC DEVICE DETECTION ---
        // ==========================================
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            if (currentDeviceMode != DeviceMode.Keyboard) SwitchDeviceMode(DeviceMode.Keyboard);
        }

        foreach (var pad in Gamepad.all)
        {
            // If ANY main face button or bumper is pressed
            if (pad.buttonSouth.wasPressedThisFrame || pad.buttonEast.wasPressedThisFrame || pad.buttonWest.wasPressedThisFrame ||
                pad.buttonNorth.wasPressedThisFrame || pad.leftShoulder.wasPressedThisFrame || pad.rightShoulder.wasPressedThisFrame)
            {
                DeviceMode mode = (pad is DualShockGamepad) ? DeviceMode.PlayStation : DeviceMode.Xbox;
                if (currentDeviceMode != mode) SwitchDeviceMode(mode);
                break;
            }
        }
        // ==========================================


        bool skipTip = false;
        if (Keyboard.current != null && Keyboard.current.xKey.wasPressedThisFrame) skipTip = true;
        if (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame) skipTip = true;

        if (currentActiveTips != null && currentActiveTips.Length > 0)
        {
            tipTimer += Time.deltaTime;

            if (tipTimer >= tipInterval || skipTip)
            {
                tipTimer = 0f;
                currentTip = (currentTip + 1) % currentActiveTips.Length;
                tipsText.text = currentActiveTips[currentTip];
            }
        }

        if (!loadingComplete)
        {
            if (currentDelay < startDelay)
            {
                currentDelay += Time.deltaTime;

                if (playerIcon != null && startPoint != null)
                {
                    playerIcon.rectTransform.position = startPoint.position;
                }

                return;
            }

            timer += Time.deltaTime;
            float progress = timer / loadDuration;

            if (loadingBarFill != null) loadingBarFill.fillAmount = progress;

            if (playerIcon != null && startPoint != null && endPoint != null)
            {
                playerIcon.rectTransform.position = Vector3.Lerp(startPoint.position, endPoint.position, progress);

                if (cherries != null && cherries.Length > 0)
                {
                    int cherriesEaten = Mathf.FloorToInt(progress * cherries.Length);
                    for (int i = 0; i < cherries.Length; i++)
                    {
                        if (i < cherriesEaten && cherries[i] != null)
                        {
                            cherries[i].SetActive(false);
                        }
                    }
                }
            }

            if (timer >= loadDuration)
            {
                loadingComplete = true;
                pressAButtonPrompt.SetActive(true);

                if (playerIcon != null && activeSitSprite != null)
                {
                    playerIcon.sprite = activeSitSprite;
                    playerIcon.rectTransform.localScale = activeSitScale;
                }
            }
            return;
        }

        if (InputManager.Instance.GetConfirmDown(1) && GameManager.Instance.isOnKeyboard)
        {
            BeginRound();
            return;
        }

        for (int i = 0; i < totalPlayers; i++)
        {
            int playerID = i + 1;
            if (!playerReady[i] && InputManager.Instance.GetConfirmDown(playerID))
            {
                playerReady[i] = true;
                readyCount++;

                if (i < playerReadyIcons.Length && playerReadyIcons[i] != null)
                {
                    playerReadyIcons[i].gameObject.SetActive(true);

                    if (GameManager.Instance.playerCustomizations.Count > i)
                    {
                        int colorIndex = GameManager.Instance.playerCustomizations[i].colorIndex;
                        if (colorIndex >= 0 && colorIndex < colorIcons.Length)
                            playerReadyIcons[i].sprite = colorIcons[colorIndex];
                    }
                }
            }
        }

        if (readyCount >= totalPlayers && totalPlayers > 0)
            BeginRound();
    }

    public void BeginRound()
    {
        if (roundStarted) return;
        roundStarted = true;

        GameObject canvas = GameObject.Find("PlayerCanvas");
        if (canvas != null) canvas.SetActive(true);

        if (levelAmbience != null)
        {
            levelAmbience.Play();
        }

        if (GameplayMusicManager.Instance != null)
        {
            GameplayMusicManager.Instance.StartMusic();
        }

        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.BeginRound();
        }

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }

        this.enabled = false;
    }
}