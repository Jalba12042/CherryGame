using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // <-- Added so we can use UI Images!
using System.Collections;

public class PlayerIndicator : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("0 = P1, 1 = P2, 2 = P3, 3 = P4")]
    public int playerIndex = 0;
    public float displayDuration = 2.5f;

    [Header("References")]
    [Tooltip("Drag your IndicatorGraphic (UI Image) here.")]
    public Image indicatorImage; // <-- Changed to Image!

    [Header("Sprites (Order by Color Index from Customization)")]
    public Sprite[] p1Sprites;
    public Sprite[] p2Sprites;
    public Sprite[] p3Sprites;
    public Sprite[] p4Sprites;

    private Coroutine hideCoroutine;
    private int myColorIndex = 0;

    void Start()
    {
        // Start completely invisible
        if (indicatorImage != null)
            indicatorImage.enabled = false;

        // Automatically fetch this player's color from the GameManager
        if (GameManager.Instance != null && GameManager.Instance.playerCustomizations.Count > playerIndex)
        {
            myColorIndex = GameManager.Instance.playerCustomizations[playerIndex].colorIndex;
        }

        SetMySprite();
    }

    void SetMySprite()
    {
        if (indicatorImage == null) return;

        Sprite[] selectedArray = null;

        if (playerIndex == 0) selectedArray = p1Sprites;
        else if (playerIndex == 1) selectedArray = p2Sprites;
        else if (playerIndex == 2) selectedArray = p3Sprites;
        else if (playerIndex == 3) selectedArray = p4Sprites;

        // Assign the correct sprite based on the player's color choice
        if (selectedArray != null && myColorIndex >= 0 && myColorIndex < selectedArray.Length)
        {
            indicatorImage.sprite = selectedArray[myColorIndex];
        }
    }

    void Update()
    {
        if (InputManager.Instance == null) return;

        int playerID = playerIndex + 1;
        bool pressed = false;

        // Check Keyboard (Tab key)
        if (InputManager.Instance.IsKeyboardPlayer(playerID))
        {
            if (Input.GetKeyDown(KeyCode.Tab)) pressed = true;
        }
        else
        {
            // Check Gamepad (Any D-Pad direction)
            Gamepad pad = InputManager.Instance.GetAssignedGamepad(playerID);
            if (pad != null)
            {
                if (pad.dpad.up.wasPressedThisFrame ||
                    pad.dpad.down.wasPressedThisFrame ||
                    pad.dpad.left.wasPressedThisFrame ||
                    pad.dpad.right.wasPressedThisFrame)
                {
                    pressed = true;
                }
            }
        }

        if (pressed)
        {
            ShowIndicator();
        }
    }

    public void ShowIndicator()
    {
        if (indicatorImage == null) return;

        indicatorImage.enabled = true;

        // If they spam the button, reset the timer so it doesn't instantly disappear
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        if (indicatorImage != null) indicatorImage.enabled = false;
    }
}
