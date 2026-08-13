using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerEscapeUI : MonoBehaviour
{
    [Header("Escape Settings")]
    public float mashFillSpeed = 0.2f;

    [Tooltip("Base amount of mash required when one player is grabbing you.")]
    public float escapeThreshold = 1f;

    [Tooltip("How much harder escaping becomes per additional grabber.")]
    public float escapeDifficultyPerGrabber = 1f;

    [Header("Player Info")]
    public int playerIndex = 0;

    [Header("UI")]
    public GameObject panelRoot;

    [Header("Dynamic Button Sprites (0 = Normal, 1 = Pressed)")]
    public Sprite[] xboxSprites = new Sprite[2];
    public Sprite[] psSprites = new Sprite[2];
    public Sprite[] keyboardSprites = new Sprite[2];

    [Header("Animation Settings")]
    public float animationSpeed = 0.15f;

    private Image fillBar;
    private TextMeshProUGUI mashText;
    private Image buttonIcon;

    private float fillAmount = 0f;
    private bool isBeingGrabbed = false;
    private PlayerInteract playerInteract;

    // Animation tracking
    private Sprite[] currentDeviceSprites;
    private float animTimer = 0f;
    private int currentSpriteIndex = 0;

    private void Start()
    {
        playerInteract = GetComponent<PlayerInteract>();

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    // =========================================================
    // START BEING GRABBED
    // =========================================================
    public void StartBeingGrabbed()
    {
        isBeingGrabbed = true;
        fillAmount = 0f;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        CacheUI();
        DetermineDeviceSprites();

        if (fillBar != null)
            fillBar.fillAmount = 0f;
    }

    // =========================================================
    // CACHE UI
    // =========================================================
    private void CacheUI()
    {
        if (panelRoot == null)
            return;

        fillBar = panelRoot.transform.Find("FillBar")?.GetComponent<Image>();
        mashText = panelRoot.transform.Find("Text (TMP)")?.GetComponent<TextMeshProUGUI>();

        // Fixed the name to match your Hierarchy screenshot exactly!
        buttonIcon = panelRoot.transform.Find("YButton")?.GetComponent<Image>();
    }

    // =========================================================
    // DEVICE DETECTION
    // =========================================================
    private void DetermineDeviceSprites()
    {
        int playerID = playerIndex + 1;

        if (InputManager.Instance.IsKeyboardPlayer(playerID))
        {
            currentDeviceSprites = keyboardSprites;
            return;
        }

        Gamepad pad = InputManager.Instance.GetAssignedGamepad(playerID);
        if (pad != null)
        {
            string padName = pad.name.ToLower();
            // Check if it's a PlayStation controller
            if (padName.Contains("dualshock") || padName.Contains("dualsense") || padName.Contains("playstation"))
            {
                currentDeviceSprites = psSprites;
            }
            else
            {
                currentDeviceSprites = xboxSprites; // Default to Xbox
            }
        }
        else
        {
            currentDeviceSprites = xboxSprites; // Fallback
        }

        // Reset animation state
        animTimer = 0f;
        currentSpriteIndex = 0;
        UpdateButtonSprite();
    }

    // =========================================================
    // STOP BEING GRABBED
    // =========================================================
    public void StopBeingGrabbed()
    {
        isBeingGrabbed = false;
        fillAmount = 0f;

        if (fillBar != null)
            fillBar.fillAmount = 0f;

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    // =========================================================
    // UPDATE
    // =========================================================
    private void Update()
    {
        if (!isBeingGrabbed)
            return;

        AnimateButtonPrompt();

        if (playerInteract == null)
            return;

        int grabberCount = playerInteract.NumberOfGrabbers;

        if (grabberCount <= 0)
        {
            StopBeingGrabbed();
            return;
        }

        // ESCAPE DIFFICULTY
        float currentEscapeThreshold = escapeThreshold + ((grabberCount - 1) * escapeDifficultyPerGrabber);

        // MASH INPUT
        if (InputManager.Instance.GetEscapeDown(playerIndex + 1))
        {
            fillAmount += mashFillSpeed;
            fillAmount = Mathf.Clamp(fillAmount, 0f, currentEscapeThreshold);

            if (fillBar != null)
            {
                fillBar.fillAmount = fillAmount / currentEscapeThreshold;
            }

            if (fillAmount >= currentEscapeThreshold)
            {
                Escape();
            }
        }
    }

    // =========================================================
    // ANIMATE BUTTON
    // =========================================================
    private void AnimateButtonPrompt()
    {
        if (currentDeviceSprites == null || currentDeviceSprites.Length < 2) return;

        animTimer += Time.deltaTime;
        if (animTimer >= animationSpeed)
        {
            animTimer = 0f;
            currentSpriteIndex = (currentSpriteIndex == 0) ? 1 : 0;
            UpdateButtonSprite();
        }
    }

    private void UpdateButtonSprite()
    {
        if (buttonIcon != null && currentDeviceSprites != null && currentDeviceSprites.Length > currentSpriteIndex)
        {
            buttonIcon.sprite = currentDeviceSprites[currentSpriteIndex];
        }
    }

    // =========================================================
    // ESCAPE
    // =========================================================
    private void Escape()
    {
        if (playerInteract == null) return;
        StopBeingGrabbed();
        playerInteract.EscapeFromAllGrabbers();
    }
}