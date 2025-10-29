using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LocalMenuController : MonoBehaviour
{
    [Header("Menu Buttons")]
    public ImageButtonSwitch[] buttons; // Assign your 2P, 3P, 4P buttons here

    private int currentIndex = 0;
    private bool canMove = true;
    private float deadzone = 0.5f;

    void Start()
    {
        HighlightCurrent();
    }

    void Update()
    {
        if (Gamepad.all.Count == 0) return;

        var gamepad = Gamepad.all[0];
        Vector2 move = gamepad.leftStick.ReadValue();

        if (canMove)
        {
            if (move.x > deadzone)
            {
                currentIndex = Mathf.Min(buttons.Length - 1, currentIndex + 1);
                HighlightCurrent();
                canMove = false;
            }
            else if (move.x < -deadzone)
            {
                currentIndex = Mathf.Max(0, currentIndex - 1);
                HighlightCurrent();
                canMove = false;
            }
        }

        if (Mathf.Abs(move.x) < 0.2f)
            canMove = true;

        if (gamepad.buttonSouth.wasPressedThisFrame)
            SelectOption(currentIndex);
    }

    void HighlightCurrent()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i == currentIndex)
                buttons[i].Highlight(true);
            else
                buttons[i].Highlight(false);
        }
    }

    // Your same logic for choosing player count + loading the next scene
    void SelectOption(int index)
    {
        int players = index + 2; // 0→2P, 1→3P, 2→4P
        GameManager.Instance.playerCount = players;
        SceneManager.LoadScene("ControllerConnectScene");
    }
}
