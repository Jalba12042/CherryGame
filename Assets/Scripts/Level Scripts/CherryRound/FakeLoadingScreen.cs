using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class FakeLoadingScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject loadingPanel;
    public Image loadingBarFill;
    public GameObject pressAButtonPrompt;
    public TMP_Text tipsText;
    public string[] tips;
    public float tipInterval = 2f;
    public TMP_Text levelTitleText;

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

    // ==========================================
    // --- THEME SETTINGS & SCALES ---
    // ==========================================
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
    public Vector3 mountainItemScale = new Vector3(1f, 1f, 1f); // Lower this to shrink the snowballs!
    public Vector3 mountainRunScale = new Vector3(1f, 1f, 1f);
    public Vector3 mountainSitScale = new Vector3(0.6f, 0.6f, 1f);
    // ==========================================

    // Internal tracking for whatever map we are currently on
    private Sprite activeRunSprite;
    private Sprite activeSitSprite;
    private Vector3 activeRunScale;
    private Vector3 activeSitScale;

    [Header("Timing")]
    public float loadDuration = 10f;
    private float timer = 0f;
    private bool loadingComplete = false;
    private float tipTimer = 0f;
    private int currentTip = 0;
    private bool roundStarted = false;

    [Header("Color Icons")]
    public Sprite[] colorIcons;

    private void Start()
    {
        ApplyMapTheme();

        foreach (var player in FindObjectsByType<Playermovement>(FindObjectsSortMode.None))
        {
            player.allowJumpInput = false;
        }

        loadingPanel.SetActive(true);
        pressAButtonPrompt.SetActive(false);
        if (loadingBarFill != null) loadingBarFill.fillAmount = 0;

        timer = 0f;
        loadingComplete = false;

        if (tips != null && tips.Length > 0)
        {
            tipsText.text = tips[0];
        }

        // Apply the correct Run Sprite and Run Scale when loading starts
        if (playerIcon != null && activeRunSprite != null)
        {
            playerIcon.sprite = activeRunSprite;
            playerIcon.rectTransform.localScale = activeRunScale;
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
        string sceneName = SceneManager.GetActiveScene().name;

        // Set Defaults (Park)
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
        }
        else if (sceneName.Contains("Mountain"))
        {
            if (levelTitleText != null) levelTitleText.text = "Level Loading: Mountain";
            currentItemSprite = mountainSnowballSprite;
            currentItemScale = mountainItemScale;

            activeRunSprite = mountainRunningSprite;
            activeSitSprite = mountainSittingSprite;
            activeRunScale = mountainRunScale;
            activeSitScale = mountainSitScale;
        }
        else
        {
            if (levelTitleText != null) levelTitleText.text = "Level Loading: Park";
        }

        // Swap the image AND the scale for all 18 objects
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

    private void Update()
    {
        if (tips != null && tips.Length > 0)
        {
            tipTimer += Time.deltaTime;
            if (tipTimer >= tipInterval)
            {
                tipTimer = 0f;
                currentTip = (currentTip + 1) % tips.Length;
                tipsText.text = tips[currentTip];
            }
        }

        if (!loadingComplete)
        {
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

                // Apply the correct Sit Sprite and Sit Scale when loading ends
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