using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Buttons (Local, Multiplayer in this order)")]
    public MenuSelectable[] buttons;   // <<— make sure this says MenuSelectable[]

    private int currentIndex = 0;
    private bool canMove = true;
    private float deadzone = 0.5f;

    void Awake()
    {
        // Optional: auto-find if you don’t want to drag
        if (buttons == null || buttons.Length == 0)
        {
            buttons = FindObjectsByType<MenuSelectable>(FindObjectsSortMode.None)
                      .OrderBy(b => b.transform.GetSiblingIndex())
                      .ToArray();
        }
    }

    void Start() => HighlightCurrent();

    void Update()
    {
        if (Gamepad.all.Count == 0 || buttons == null || buttons.Length == 0) return;

        var gamepad = Gamepad.all[0];

        Vector2 move = gamepad.leftStick.ReadValue();
        float xInput = move.x;
        if (gamepad.dpad.left.wasPressedThisFrame) xInput = -1;
        if (gamepad.dpad.right.wasPressedThisFrame) xInput = 1;

        if (canMove)
        {
            if (xInput > deadzone)
            {
                currentIndex = Mathf.Min(buttons.Length - 1, currentIndex + 1);
                HighlightCurrent();
                canMove = false;
            }
            else if (xInput < -deadzone)
            {
                currentIndex = Mathf.Max(0, currentIndex - 1);
                HighlightCurrent();
                canMove = false;
            }
        }
        if (Mathf.Abs(xInput) < 0.2f) canMove = true;

        if (gamepad.buttonSouth.wasPressedThisFrame)
            buttons[currentIndex].Activate();
    }

    void HighlightCurrent()
    {
        for (int i = 0; i < buttons.Length; i++)
            buttons[i].Highlight(i == currentIndex);
    }
}