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
            buttons = FindObjectsOfType<MenuSelectable>(true)
                      .OrderBy(b => b.transform.GetSiblingIndex())
                      .ToArray();
        }
    }

    void Start() => HighlightCurrent();

    void Update()
    {
        if (Gamepad.current == null || buttons == null || buttons.Length == 0) return;

        Vector2 move = Gamepad.current.leftStick.ReadValue();
        float xInput = move.x;
        if (Gamepad.current.dpad.left.wasPressedThisFrame) xInput = -1;
        if (Gamepad.current.dpad.right.wasPressedThisFrame) xInput = 1;

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

        if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            buttons[currentIndex].Activate();
    }

    void HighlightCurrent()
    {
        for (int i = 0; i < buttons.Length; i++)
            buttons[i].Highlight(i == currentIndex);
    }
}