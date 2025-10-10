using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Menu Buttons")]
    public Button[] menuButtons; // Array of buttons in the menu
    public Image[] highlightImages; // Array of highlight images (same order as buttons)

    private int currentIndex = 0;
    private bool canMove = true;
    private float deadzone = 0.5f;

    void Start()
    {
        HighlightButton();
    }

    void Update()
    {
        if (Gamepad.all.Count == 0) return;

        var gamepad = Gamepad.all[0];
        Vector2 move = gamepad.leftStick.ReadValue();

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

        if (Mathf.Abs(move.y) < 0.2f)
            canMove = true;

        if (gamepad.buttonSouth.wasPressedThisFrame)
            SelectOption(currentIndex);
    }

    void HighlightButton()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            // Change color
            ColorBlock colors = menuButtons[i].colors;
            colors.normalColor = (i == currentIndex) ? Color.yellow : Color.white;
            menuButtons[i].colors = colors;

            // Show/hide highlight image
            if (highlightImages != null && i < highlightImages.Length && highlightImages[i] != null)
            {
                highlightImages[i].enabled = (i == currentIndex);
            }
        }
    }

    void SelectOption(int index)
    {
        int players = index + 2;
        GameManager.Instance.playerCount = players;
        SceneManager.LoadScene("ControllerConnectScene");
    }
}
