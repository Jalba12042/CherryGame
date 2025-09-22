using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public Button[] menuButtons; // Array of buttons in the menu
    private int currentIndex = 0;     // Tracks which button is currently highlighted


    private bool canMove = true;     // Prevents input from being read too many times while joystick is held
    private float deadzone = 0.5f;   // Threshold for how far the joystick must be tilted before registering a move


    void Start()
    {
        HighlightButton();   // At the start of the scene, highlight the first button

    }

    void Update()
    {
        // If no controller is connected, stop here
        if (Gamepad.all.Count == 0) return;

        var gamepad = Gamepad.all[0];
        Vector2 move = gamepad.leftStick.ReadValue(); // Read the left stick input


        // Only allow movement if stick is past deadzone and canMove is true
        if (canMove)
        {
            if (move.y > deadzone)
            {
                currentIndex = Mathf.Max(0, currentIndex - 1);
                HighlightButton();
                canMove = false;
            }
            else if (move.y < -deadzone)
            {
                currentIndex = Mathf.Min(menuButtons.Length - 1, currentIndex + 1);
                HighlightButton();
                canMove = false;
            }
        }

        // Reset canMove when stick goes back to neutral
        if (Mathf.Abs(move.y) < 0.2f)
        {
            canMove = true;
        }

        // Confirm selection
        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            SelectOption(currentIndex);
        }
    }

    // Highlights the currently selected button by changing its color
    void HighlightButton()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            ColorBlock colors = menuButtons[i].colors;
            colors.normalColor = (i == currentIndex) ? Color.yellow : Color.white;
            menuButtons[i].colors = colors;
        }
    }

    // Runs when player presses confirm (A button)
    void SelectOption(int index)
    {
        int players = index + 2;
        GameManager.Instance.playerCount = players;
        SceneManager.LoadScene("ControllerConnectScene");
    }
}
