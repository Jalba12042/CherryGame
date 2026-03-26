using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class FakeLoadingScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject loadingPanel;
    public Image loadingBarFill; // We keep this so the boss's code doesn't break!
    public GameObject pressAButtonPrompt;
    public TMP_Text tipsText;
    public string[] tips;
    public float tipInterval = 2f;

    // NEW: Pac-Man Style Variables
    [Header("Pac-Man Loading Elements")]
    public Image playerIcon;        // The puppet moving across
    public RectTransform startPoint; // Empty object on the left
    public RectTransform endPoint;   // Empty object on the right
    public GameObject[] cherries;    // Array for your 13 cherries
    public Sprite runningSprite;     // The normal moving puppet image
    public Sprite sittingSprite;     // The image to show when loading is done

    [Header("Timing")]
    public float loadDuration = 10f;

    private float timer = 0f;
    private bool loadingComplete = false;
    private float tipTimer = 0f;
    private int currentTip = 0;

    private bool roundStarted = false;

    private void Start()
    {
        // Disable jump for all players during loading
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

        // NEW: Reset the Pac-Man stuff when the scene loads
        if (playerIcon != null && runningSprite != null)
        {
            playerIcon.sprite = runningSprite;
        }
        if (cherries != null)
        {
            foreach (GameObject cherry in cherries)
            {
                if (cherry != null) cherry.SetActive(true);
            }
        }
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

        // STEP 1: Animate loading bar AND the Pac-Man Puppet
        if (!loadingComplete)
        {
            timer += Time.deltaTime;

            // This 'progress' number goes from 0.0 to 1.0
            float progress = timer / loadDuration;

            if (loadingBarFill != null) loadingBarFill.fillAmount = progress;

            // NEW: Move the puppet and eat cherries
            if (playerIcon != null && startPoint != null && endPoint != null)
            {
                // Slide across the screen
                playerIcon.rectTransform.position = Vector3.Lerp(startPoint.position, endPoint.position, progress);

                // Figure out how many cherries to turn off
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

                // NEW: Change to the sitting sprite when finished!
                if (playerIcon != null && sittingSprite != null)
                {
                    playerIcon.sprite = sittingSprite;
                }
            }
            return;
        }

        // STEP 2: Wait for "A" on ANY player's gamepad
        foreach (var pad in UnityEngine.InputSystem.Gamepad.all)
        {
            if (pad.buttonSouth.wasPressedThisFrame)
            {
                BeginRound();
                break;
            }
        }
    }

    public void BeginRound()
    {
        if (roundStarted) return;
        roundStarted = true;

        if (RoundManager.Instance.currRound.startTimerUI != null)
            RoundManager.Instance.currRound.startTimerUI.SetActive(true);

        RoundManager.Instance.BeginRound();
        loadingPanel.SetActive(false);
        this.enabled = false; // prevent duplicate calls
    }
}
