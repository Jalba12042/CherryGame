using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControllerConnect : MonoBehaviour
{
    [Header("Slots (assign in Inspector in order: Blue, Green, Red, etc.)")]
    public Image[] slots;

    [Header("Waiting squares (same number as max controllers)")]
    public Image[] waitingSquares;

    [Header("Sprites")]
    public Sprite whiteController; // default controller waiting
    public Sprite grayController;  // empty slot / waiting spot
    public Sprite[] slotSprites;   // assign Blue, Green, Red, etc. in order

    [Header("UI")]
    public Button playButton;

    private Gamepad[] controllers;
    private int[] controllerPositions;
    private bool[] stickLocked; // lock horizontal movement per controller

    void Start()
    {
        playButton.gameObject.SetActive(false);

        controllers = Gamepad.all.ToArray();
        controllerPositions = new int[controllers.Length];
        stickLocked = new bool[controllers.Length];

        ResetUI();
    }

    void Update()
    {
        for (int i = 0; i < controllers.Length && i < waitingSquares.Length; i++)
        {
            if (controllers[i] != null)
                HandleController(controllers[i], ref controllerPositions[i], waitingSquares[i], i);
        }

        // Only consider slots on the active canvas
        Image[] activeSlots = slots.Where(s => s.gameObject.activeInHierarchy).ToArray();

        // Check if all active slots have a controller assigned
        bool allSlotsFilled = true;
        for (int i = 0; i < activeSlots.Length; i++)
        {
            int slotIndex = System.Array.IndexOf(slots, activeSlots[i]) + 1; // 1-based index
            if (!controllerPositions.Contains(slotIndex))
            {
                allSlotsFilled = false;
                break;
            }
        }

        // Enable play button only when all slots are filled
        playButton.gameObject.SetActive(allSlotsFilled);
    }

    void HandleController(Gamepad gamepad, ref int pos, Image waitingSquare, int controllerIndex)
    {
        Vector2 move = gamepad.leftStick.ReadValue();

        // Enter slot from waiting
        if (pos == 0 && move.y > 0.5f)
        {
            if (move.x < -0.5f) TryEnterSlot(ref pos, 1, waitingSquare);
            else if (move.x > 0.5f && slots.Length > 1) TryEnterSlot(ref pos, 2, waitingSquare);
            else if (move.x == 0 && slots.Length > 2) TryEnterSlot(ref pos, 3, waitingSquare);
        }

        // Switch slots horizontally
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
                    stickLocked[controllerIndex] = true; // lock until stick released
                }
            }

            // Release lock when stick back to neutral
            if (Mathf.Abs(move.x) < 0.2f)
            {
                stickLocked[controllerIndex] = false;
            }
        }

        // Back to waiting
        if (pos != 0 && move.y < -0.5f)
        {
            ClearSlot(pos);
            pos = 0;
            waitingSquare.sprite = whiteController;
        }
    }

    void TryEnterSlot(ref int pos, int slotIndex, Image waitingSquare)
    {
        if (controllerPositions.Contains(slotIndex)) return;

        pos = slotIndex;
        slots[slotIndex - 1].sprite = slotSprites[slotIndex - 1];
        waitingSquare.sprite = grayController;
    }

    void TrySwitchSlot(ref int pos, int newSlot, Image waitingSquare, int controllerIndex)
    {
        if (controllerPositions.Where((p, idx) => idx != controllerIndex).Contains(newSlot)) return;

        ClearSlot(pos);
        pos = newSlot;
        slots[newSlot - 1].sprite = slotSprites[newSlot - 1];
        waitingSquare.sprite = grayController;
    }

    void ClearSlot(int slotIndex)
    {
        slots[slotIndex - 1].sprite = grayController;
    }

    public void OnPlayPressed()
    {
        if (controllers.Length > 0 && controllers[0].buttonSouth.wasPressedThisFrame)
        {
            GameManager.Instance.playerCount = slots.Length;
            SceneManager.LoadScene("GameScene");
        }
    }

    void ResetUI()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].sprite = grayController;

        for (int i = 0; i < waitingSquares.Length; i++)
            waitingSquares[i].sprite = whiteController;
    }
}
