using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControllerConnect : MonoBehaviour
{
    [Header("Slots (assign in Inspector in order: Blue, Green, Red, etc.)")]
    public Image[] slots;   // Target slots for players (e.g. 2 slots for 2P, 3 slots for 3P)

    [Header("Waiting squares (same number as max controllers)")]
    public Image[] waitingSquares;

    [Header("Sprites")]
    public Sprite whiteController; // default controller waiting
    public Sprite grayController;  // empty slot / waiting spot
    public Sprite[] slotSprites;   // assign Blue, Green, Red, etc. in order

    [Header("UI")]
    public Button playButton;

    private Gamepad[] controllers;        // Connected controllers
    private int[] controllerPositions;    // Where each controller is: 0 = waiting, 1 = slot1, 2 = slot2, etc.

    private bool[] stickLocked;
    private float targetSpriteSize = 100f; // desired size for all slot sprites


    void Start()
    {
        playButton.gameObject.SetActive(false);

        // Assign controllers based on what is connected
        controllers = Gamepad.all.ToArray();

        // Initialize positions (0 = waiting)
        controllerPositions = new int[controllers.Length];
        stickLocked = new bool[controllers.Length]; // same size as controllers

        ResetUI();
    }

    void Update()
    {
        for (int i = 0; i < controllers.Length && i < waitingSquares.Length; i++)
        {
            if (controllers[i] != null)
                HandleController(controllers[i], ref controllerPositions[i], waitingSquares[i], i);
        }

        // Enable Play only if all controllers are in unique non-waiting slots
        bool allSlotsFilled = true;
        for (int i = 0; i < slots.Length; i++)
        {
            if (!controllerPositions.Contains(i + 1)) // slot index starts at 1
            {
                allSlotsFilled = false;
                break;
            }
        }

        playButton.gameObject.SetActive(allSlotsFilled);
    }

    void HandleController(Gamepad gamepad, ref int pos, Image waitingSquare, int controllerIndex)
    {
        Vector2 move = gamepad.leftStick.ReadValue();

        // === Move to a slot ===
        if (pos == 0 && move.y > 0.5f)
        {
            // Left vs Right stick decides which slot
            if (move.x < -0.5f) TryEnterSlot(ref pos, 1, waitingSquare);
            else if (move.x > 0.5f && slots.Length > 1) TryEnterSlot(ref pos, 2, waitingSquare);
            else if (move.x == 0 && slots.Length > 2) TryEnterSlot(ref pos, 3, waitingSquare);
        }

        // === Switch slots horizontally ===
        // === Switch slots horizontally ===
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

            // Release lock when stick goes back near neutral
            if (Mathf.Abs(move.x) < 0.2f)
            {
                stickLocked[controllerIndex] = false;
            }
        }

        // === Back to waiting (Down stick) ===
        if (pos != 0 && move.y < -0.5f)
        {
            ClearSlot(pos);
            pos = 0;
            waitingSquare.sprite = whiteController;
        }
    }

  void TryEnterSlot(ref int pos, int slotIndex, Image waitingSquare)
{
    // Prevent entering occupied slot
    if (controllerPositions.Contains(slotIndex)) return;

    pos = slotIndex;
    AssignSlotSprite(slotIndex, waitingSquare);
}

void TrySwitchSlot(ref int pos, int newSlot, Image waitingSquare, int controllerIndex)
{
    // Prevent switching into an occupied slot (ignore self)
    if (controllerPositions.Where((p, idx) => idx != controllerIndex).Contains(newSlot)) return;

    ClearSlot(pos);
    pos = newSlot;
    AssignSlotSprite(newSlot, waitingSquare);
}

void AssignSlotSprite(int slotIndex, Image waitingSquare)
{
    // Set the correct sprite for the slot
    slots[slotIndex - 1].sprite = slotSprites[slotIndex - 1];
    waitingSquare.sprite = grayController;

    // Set native size then scale to target
    slots[slotIndex - 1].SetNativeSize();
    RectTransform rt = slots[slotIndex - 1].rectTransform;
    float maxDim = Mathf.Max(rt.sizeDelta.x, rt.sizeDelta.y);
    float scale = targetSpriteSize / maxDim;
    rt.sizeDelta = rt.sizeDelta * scale;
}

    void ClearSlot(int slotIndex)
    {
        slots[slotIndex - 1].sprite = grayController;
    }

    public void OnPlayPressed()
    {
        // Only Player 1 (controller[0]) can confirm
        if (controllers.Length > 0 && controllers[0].buttonSouth.wasPressedThisFrame)
        {
            GameManager.Instance.playerCount = slots.Length;
            SceneManager.LoadScene("GameScene");
        }
    }

    void ResetUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].sprite = grayController;
        }

        for (int i = 0; i < waitingSquares.Length; i++)
        {
            waitingSquares[i].sprite = whiteController;
        }
    }
}