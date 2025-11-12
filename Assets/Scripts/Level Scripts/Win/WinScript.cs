using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinScript : MonoBehaviour
{
    public TMP_Text winnerText; // assign in Inspector to display winner
    public Button[] menuButtons; // Shop button etc.
    private int currentIndex = 0;

    private bool canMove = true;
    private float deadzone = 0.5f;

    [SerializeField] private string shopSceneName;

    // NEW: store the winning player
    public static List<int> winningPlayers;

    void Start()
    {
        // Show winner text
        if (winnerText != null)
        {
            if (winningPlayers.Count != 1)
            {
                string winners = "Players ";
                for (int i = 0; i < winningPlayers.Count; i++)
                {
                    winners += winningPlayers[i]+1;
                    if (i == winningPlayers.Count-1)
                    {
                        winners += " ";
                    }
                    else
                    {
                        winners += ", ";
                    }
                }
                winners += "Tied!";

                winnerText.text = winners;
            }
            else
            {
                winnerText.text = $"Player {winningPlayers[0]+1} Wins!";
            }
        }

        HighlightButton();
    }

    void Update()
    {
        if (Gamepad.all.Count == 0) return;
        var gamepad = Gamepad.all[0];
        Vector2 move = gamepad.leftStick.ReadValue();

        // Navigation
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
        {
            canMove = true;
        }

        // Confirm selection
        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            menuButtons[currentIndex].onClick.Invoke();
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

    public void GoToShop()
    {
        // This assumes your shop scene is literally called "Shop"
        SceneManager.LoadScene(shopSceneName);
    }
}
