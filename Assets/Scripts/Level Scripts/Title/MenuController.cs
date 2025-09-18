using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public Button[] menuButtons; // assign 2P, 3P, 4P in inspector
    private int currentIndex = 0;

    void Start()
    {
        HighlightButton();
    }

    void Update()
    {
        if (Gamepad.all.Count == 0) return;

        var gamepad = Gamepad.all[0]; // take first connected controller for menu

        Vector2 move = gamepad.leftStick.ReadValue();

        // navigate menu up/down
        if (move.y > 0.5f)
        {
            currentIndex = Mathf.Max(0, currentIndex - 1);
            HighlightButton();
        }
        else if (move.y < -0.5f)
        {
            currentIndex = Mathf.Min(menuButtons.Length - 1, currentIndex + 1);
            HighlightButton();
        }

        // confirm selection with "A" (south button)
        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            SelectOption(currentIndex);
        }
    }

    void HighlightButton()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            ColorBlock colors = menuButtons[i].colors;
            colors.normalColor = (i == currentIndex) ? Color.yellow : Color.white;
            menuButtons[i].colors = colors;
        }
    }

    void SelectOption(int index)
    {
        int players = index + 2; // since index 0 = 2 players, index 1 = 3 players, etc.
        GameManager.Instance.playerCount = players;
        SceneManager.LoadScene("ControllerConnectScene");
    }
}
