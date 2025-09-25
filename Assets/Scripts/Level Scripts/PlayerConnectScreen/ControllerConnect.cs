using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControllerConnect : MonoBehaviour
{
    [Header("Player Canvas")]
    public GameObject playerCanvas; // assign the active player canvas (2P, 3P, 4P)

    [Header("Slots and Waiting Squares (assign children of the active canvas)")]
    public Image[] slots;           // only slots for this canvas
    public Image[] waitingSquares;  // only waiting squares for this canvas

    [Header("Sprites")]
    public Sprite whiteController;  // waiting
    public Sprite grayController;   // empty slot
    public Sprite[] slotSprites;    // Blue, Red, etc.

    [Header("UI")]
    public Button playButton;

    private class ActiveController
    {
        public Gamepad gamepad;
        public int position = 0; // 0 = waiting, 1+ = slot index
        public bool horizontalLocked = false;
        public bool verticalLocked = false;
        public Image waitingSquare;
    }

    private List<ActiveController> activeControllers = new List<ActiveController>();

    void OnEnable()
    {
        if (playButton != null)
            playButton.gameObject.SetActive(false);

        ResetUI();
    }

    void Start()
    {
        // Only track controllers for active waiting squares
        activeControllers.Clear();
        int activeCount = Mathf.Min(waitingSquares.Length, Gamepad.all.Count);

        for (int i = 0; i < waitingSquares.Length && i < activeCount; i++)
        {
            activeControllers.Add(new ActiveController
            {
                gamepad = Gamepad.all[i],
                waitingSquare = waitingSquares[i]
            });
        }

        Debug.Log($"Active controllers initialized: {activeControllers.Count}");
    }

    void Update()
    {
        // Refresh gamepad references in case of connection changes
        for (int i = 0; i < activeControllers.Count && i < Gamepad.all.Count; i++)
        {
            activeControllers[i].gamepad = Gamepad.all[i];
        }

        // Handle input for each active controller
        foreach (var ctrl in activeControllers)
        {
            if (ctrl.gamepad != null)
                HandleController(ctrl);
        }

        // Count filled slots
        int filledSlots = activeControllers.Count(c => c.position > 0);

        // Debug: show controller positions
        string status = "";
        for (int i = 0; i < activeControllers.Count; i++)
            status += $"C{i}: Pos={activeControllers[i].position} | ";

        Debug.Log($"Filled Slots: {filledSlots}/{slots.Length} | {status}");

        // Enable Play button only when all slots are filled
        if (playButton != null)
            playButton.gameObject.SetActive(filledSlots == slots.Length);
    }

    void HandleController(ActiveController ctrl)
    {
        Vector2 move = ctrl.gamepad.leftStick.ReadValue();

        // Enter slot from waiting
        if (ctrl.position == 0 && move.y > 0.3f && !ctrl.verticalLocked)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (!activeControllers.Any(c => c.position == i + 1))
                {
                    ctrl.position = i + 1;
                    ctrl.waitingSquare.sprite = grayController;
                    slots[i].sprite = slotSprites[i];
                    Debug.Log($"Controller entered slot {ctrl.position}");
                    break;
                }
            }
            ctrl.verticalLocked = true;
        }

        // Switch slots horizontally
        if (ctrl.position > 0)
        {
            if (!ctrl.horizontalLocked)
            {
                int targetSlot = ctrl.position;
                if (move.x < -0.5f) targetSlot = Mathf.Max(1, ctrl.position - 1);
                if (move.x > 0.5f) targetSlot = Mathf.Min(slots.Length, ctrl.position + 1);

                if (targetSlot != ctrl.position &&
                    !activeControllers.Any(c => c != ctrl && c.position == targetSlot))
                {
                    slots[ctrl.position - 1].sprite = grayController;
                    slots[targetSlot - 1].sprite = slotSprites[targetSlot - 1];
                    ctrl.position = targetSlot;
                    ctrl.waitingSquare.sprite = grayController;
                    Debug.Log($"Controller switched to slot {ctrl.position}");
                }
                ctrl.horizontalLocked = true;
            }
        }

        // Move back to waiting
        if (ctrl.position > 0 && move.y < -0.3f && !ctrl.verticalLocked)
        {
            slots[ctrl.position - 1].sprite = grayController;
            ctrl.position = 0;
            ctrl.waitingSquare.sprite = whiteController;
            Debug.Log("Controller returned to waiting");
            ctrl.verticalLocked = true;
        }

        // Release stick locks
        if (Mathf.Abs(move.x) < 0.2f) ctrl.horizontalLocked = false;
        if (Mathf.Abs(move.y) < 0.2f) ctrl.verticalLocked = false;
    }

    public void OnPlayPressed()
    {
        SceneManager.LoadScene("GameScene");
    }

    void ResetUI()
    {
        foreach (var slot in slots)
            slot.sprite = grayController;

        foreach (var wait in waitingSquares)
            wait.sprite = whiteController;
    }
}