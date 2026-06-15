using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FakeLoadingScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject loadingPanel;
    public Image loadingBarFill;
    public GameObject pressAButtonPrompt;
    public TMP_Text tipsText;
    public string[] tips;
    public float tipInterval = 2f;


    [Header("Ready Up UI")]
    public Image[] playerReadyIcons;

    private bool[] playerReady = new bool[4];
    private int totalPlayers;
    private int readyCount;

    // NEW: We added the slot for your Timer Manager right here!
    public TimerUIManager timerManager;

    [Header("Pac-Man Loading Elements")]
    public Image playerIcon;
    public RectTransform startPoint;
    public RectTransform endPoint;
    public GameObject[] cherries;
    public Sprite runningSprite;
    public Sprite sittingSprite;

    public Vector3 runningScale = new Vector3(1f, 1f, 1f);
    public Vector3 sittingScale = new Vector3(0.6f, 0.6f, 1f);

    [Header("Timing")]
    public float loadDuration = 10f;

    private float timer = 0f;
    private bool loadingComplete = false;
    private float tipTimer = 0f;
    private int currentTip = 0;

    private bool roundStarted = false;

    [Header("Color Icons")]
    public Sprite[] colorIcons; // index matches colorIndex

    private void Start()
    {
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

        if (playerIcon != null && runningSprite != null)
        {
            playerIcon.sprite = runningSprite;
            playerIcon.rectTransform.localScale = runningScale;
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

        // Reset UI + state
        for (int i = 0; i < playerReadyIcons.Length; i++)
        {
            if (playerReadyIcons[i] != null)
            {
                // ❌ DON'T show them yet
                playerReadyIcons[i].gameObject.SetActive(false);
            }

            playerReady[i] = false;
        }

        readyCount = 0;
    }

    private void Update()
    {
        // rotate helpful tips
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

        // Animate loading bar AND the Pac-Man Puppet
        if (!loadingComplete)
        {
            timer += Time.deltaTime;
            float progress = timer / loadDuration;

            if (loadingBarFill != null) loadingBarFill.fillAmount = progress;

            // Move the puppet and eat cherries
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

                // Change to the sitting sprite AND shrink it when finished!
                if (playerIcon != null && sittingSprite != null)
                {
                    playerIcon.sprite = sittingSprite;
                    playerIcon.rectTransform.localScale = sittingScale;
                }
            }
            return;
        }

        // Keyboard fallback
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

        // NEW: Tell the timer to reveal itself the exact second you press the button!
        if (timerManager != null)
        {
            timerManager.RevealTimer();
        }

        // NEW: Safety checks so testing scenes directly doesn't crash Unity
        if (RoundManager.Instance != null && RoundManager.Instance.currRound != null)
        {
            if (RoundManager.Instance.currRound.startTimerUI != null)
                RoundManager.Instance.currRound.startTimerUI.SetActive(true);
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