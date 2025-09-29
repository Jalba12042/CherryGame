using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControllerConnect : MonoBehaviour
{
    // UI slots where players can assign themselves (e.g. Blue, Red, etc.)
    [Header("Slots (assign in Inspector in order: Blue, Green, Red, etc.)")]
    public Image[] slots;

    // Waiting areas for controllers before they join a slot
    [Header("Waiting squares (same number as max controllers)")]
    public Image[] waitingSquares;

    // Sprites for controller icons
    [Header("Sprites")]
    public Sprite whiteController; // default icon
    public Sprite grayController;  // empty slot
    public Sprite[] slotSprites;   // colored icons for each player

    // Play button that appears when all slots are filled
    [Header("UI")]
    public Button playButton;

    // Canvas this controller connect belongs to
    [Header("Canvas Reference")]
    public Canvas playerCanvas;

    // Internal controller tracking
    private Gamepad[] controllers;
    private int[] controllerPositions; // which slot each controller is in
    private bool[] stickLocked;        // prevents fast horizontal switching
    private bool gameStarted = false;  // prevents multiple scene loads

    IEnumerator Start()
    {
        yield return null; // wait for canvas setup

        // Disable if canvas is inactive or player count doesn't match
        if (playerCanvas == null || !playerCanvas.gameObject.activeInHierarchy ||
            (GameManager.Instance != null && GameManager.Instance.playerCount != slots.Length))
        {
            gameObject.SetActive(false);
            yield break;
        }

        // Hide play button at start
        if (playButton != null)
            playButton.gameObject.SetActive(false);

        // Setup controller tracking
        controllers = Gamepad.all.ToArray();
        controllerPositions = new int[controllers.Length];
        stickLocked = new bool[controllers.Length];

        ResetUI(); // reset slot visuals
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        // Skip if arrays aren't ready
        if (controllers == null || controllerPositions == null || waitingSquares == null)
            return;

        // Loop through all connected controllers
        int count = Mathf.Min(controllers.Length, waitingSquares.Length);
        for (int i = 0; i < count; i++)
        {
            if (controllers[i] != null && waitingSquares[i] != null)
                HandleController(controllers[i], ref controllerPositions[i], waitingSquares[i], i);
        }

        // Check how many slots are filled
        int filledSlots = controllerPositions.Distinct().Count(p => p > 0);
        int requiredSlots = slots.Length;

        // Show play button if all slots are filled
        if (playButton != null)
            playButton.gameObject.SetActive(filledSlots == requiredSlots);

        // Let Player 1 press A to start the game
        if (!gameStarted && playButton != null && playButton.gameObject.activeSelf && controllerPositions.Length > 0)
        {
            for (int i = 0; i < controllerPositions.Length; i++)
            {
                if (controllerPositions[i] == 1) // slot 1 is Player 1
                {
                    var gamepad = controllers[i];
                    if (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame)
                    {
                        gameStarted = true;
                        OnPlayPressed();
                    }
                }
            }
        }
    }

    // Handles movement and slot assignment for each controller
    void HandleController(Gamepad gamepad, ref int pos, Image waitingSquare, int controllerIndex)
    {
        Vector2 move = gamepad.leftStick.ReadValue();

        // Move into a slot
        if (pos == 0 && move.y > 0.5f)
        {
            if (move.x < -0.5f) TryEnterSlot(ref pos, 1, waitingSquare, controllerIndex);
            else if (move.x > 0.5f && slots.Length > 1) TryEnterSlot(ref pos, 2, waitingSquare, controllerIndex);
            else if (Mathf.Abs(move.x) < 0.2f && slots.Length > 2) TryEnterSlot(ref pos, 3, waitingSquare, controllerIndex);
        }

        // Switch slots left/right
        if (pos > 0)
        {
            if (!stickLocked[controllerIndex])
            {
                int targetSlot = pos;

                if (move.x < -0.5f) targetSlot = Mathf.Max(1, pos - 1);
                else if (move.x > 0.5f) targetSlot = Mathf.Min(slots.Length, pos + 1);

                if (targetSlot != pos)
                {
                    TrySwitchSlot(ref pos, targetSlot, waitingSquare, controllerIndex);
                    stickLocked[controllerIndex] = true;
                }
            }

            // Unlock stick when neutral
            if (Mathf.Abs(move.x) < 0.2f)
                stickLocked[controllerIndex] = false;
        }

        // Leave slot and return to waiting
        if (pos != 0 && move.y < -0.5f)
        {
            ClearSlot(pos);
            pos = 0;
            waitingSquare.sprite = whiteController;
        }
    }

    // Assign controller to a slot if it's not taken
    void TryEnterSlot(ref int pos, int slotIndex, Image waitingSquare, int controllerIndex)
    {
        bool slotTaken = false;
        for (int i = 0; i < controllerPositions.Length; i++)
        {
            if (i != controllerIndex && controllerPositions[i] == slotIndex)
            {
                slotTaken = true;
                break;
            }
        }
        if (slotTaken) return;

        pos = slotIndex;
        slots[slotIndex - 1].sprite = slotSprites[slotIndex - 1];
        waitingSquare.sprite = grayController;
    }

    // Switch controller to a different slot
    void TrySwitchSlot(ref int pos, int newSlot, Image waitingSquare, int controllerIndex)
    {
        bool slotTaken = false;
        for (int i = 0; i < controllerPositions.Length; i++)
        {
            if (i != controllerIndex && controllerPositions[i] == newSlot)
            {
                slotTaken = true;
                break;
            }
        }
        if (slotTaken) return;

        ClearSlot(pos);
        pos = newSlot;
        slots[newSlot - 1].sprite = slotSprites[newSlot - 1];
        waitingSquare.sprite = grayController;
    }

    // Reset slot to empty
    void ClearSlot(int slotIndex)
    {
        slots[slotIndex - 1].sprite = grayController;
    }

    // Called when Player 1 presses A to start the game
    public void OnPlayPressed()
    {
        GameManager.Instance.playerCount = slots.Length;
        SceneManager.LoadScene(GameManager.Instance.firstScene);
    }

    // Resets all slot and waiting visuals
    void ResetUI()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].sprite = grayController;

        for (int i = 0; i < waitingSquares.Length; i++)
            waitingSquares[i].sprite = whiteController;
    }
}
