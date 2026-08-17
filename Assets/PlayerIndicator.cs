using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class PlayerIndicator : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Leave this alone! The script automatically finds the right player now.")]
    public int playerIndex = 0;
    public float displayDuration = 2.5f;

    [Header("References")]
    [Tooltip("Drag your PlayerIcon (UI Image) here.")]
    public Image indicatorImage;

    [Header("Sprites (Order by Color Index from Customization)")]
    public Sprite[] p1Sprites;
    public Sprite[] p2Sprites;
    public Sprite[] p3Sprites;
    public Sprite[] p4Sprites;

    private Coroutine hideCoroutine;
    private int myColorIndex = 0;
    private bool isInitialized = false;

    void Start()
    {
        // Start completely invisible 
        if (indicatorImage != null)
            indicatorImage.gameObject.SetActive(false);
    }

    void InitializePlayer()
    {
        // 1. Ask the movement script which player this actually is!
        Playermovement movementScript = GetComponent<Playermovement>();
        if (movementScript != null)
        {
            playerIndex = movementScript.playerIndex;
        }

        // 2. Fetch this specific player's color from the GameManager
        if (GameManager.Instance != null && GameManager.Instance.playerCustomizations.Count > playerIndex)
        {
            myColorIndex = GameManager.Instance.playerCustomizations[playerIndex].colorIndex;
        }

        SetMySprite();
        isInitialized = true;
    }

    void SetMySprite()
    {
        if (indicatorImage == null) return;

        Sprite[] selectedArray = null;

        if (playerIndex == 0) selectedArray = p1Sprites;
        else if (playerIndex == 1) selectedArray = p2Sprites;
        else if (playerIndex == 2) selectedArray = p3Sprites;
        else if (playerIndex == 3) selectedArray = p4Sprites;

        if (selectedArray != null && myColorIndex >= 0 && myColorIndex < selectedArray.Length)
        {
            indicatorImage.sprite = selectedArray[myColorIndex];
        }
    }

    void Update()
    {
        // Wait until the spawner tells us who we are before assigning colors
        if (!isInitialized)
        {
            InitializePlayer();
        }

        int playerID = playerIndex + 1;
        bool pressed = false;

        // ==========================================
        // 1. KEYBOARD CHECK
        // ==========================================
        bool isOnKeyboard = false;

        if (InputManager.Instance != null && InputManager.Instance.IsKeyboardPlayer(playerID))
        {
            isOnKeyboard = true;
        }
        else if (playerID == 1 && (InputManager.Instance == null || !InputManager.Instance.IsAssigned(1)))
        {
            isOnKeyboard = true;
        }

        if (isOnKeyboard && Input.GetKeyDown(KeyCode.Tab))
        {
            pressed = true;
        }

        // ==========================================
        // 2. GAMEPAD CHECK
        // ==========================================
        Gamepad myPad = null;

        if (InputManager.Instance != null)
        {
            myPad = InputManager.Instance.GetAssignedGamepad(playerID);
        }

        if (myPad == null && Gamepad.all.Count >= playerID)
        {
            myPad = Gamepad.all[playerIndex];
        }

        if (myPad != null)
        {
            if (myPad.dpad.up.wasPressedThisFrame || myPad.dpad.down.wasPressedThisFrame ||
                myPad.dpad.left.wasPressedThisFrame || myPad.dpad.right.wasPressedThisFrame)
            {
                pressed = true;
            }
        }

        // ==========================================
        // 3. SHOW THE INDICATOR
        // ==========================================
        if (pressed)
        {
            ShowIndicator();
        }
    }

    public void ShowIndicator()
    {
        if (indicatorImage == null) return;

        indicatorImage.gameObject.SetActive(true);

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        if (indicatorImage != null) indicatorImage.gameObject.SetActive(false);
    }
}