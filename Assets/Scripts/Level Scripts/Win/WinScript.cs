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

    public static List<int> winningPlayers;

    // NEW: Add slots for your puppet animations
    public GameObject redPuppetAnim;
    public GameObject bluePuppetAnim;

    void Start()
    {
        // NEW: Make sure both are hidden at the very start
        if (redPuppetAnim != null) redPuppetAnim.SetActive(false);
        if (bluePuppetAnim != null) bluePuppetAnim.SetActive(false);

        // Show winner text
        if (winnerText != null)
        {
            if (winningPlayers.Count != 1)
            {
                // Tie logic
                string winners = "Players ";
                for (int i = 0; i < winningPlayers.Count; i++)
                {
                    winners += winningPlayers[i] + 1;
                    if (i == winningPlayers.Count - 1)
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
                // Someone won!
                winnerText.text = $"Player {winningPlayers[0] + 1} Wins!";

                // NEW: Turn on the correct puppet based on who won
                // winningPlayers[0] == 0 means Player 1 (Red)
                // winningPlayers[0] == 1 means Player 2 (Blue)

                if (winningPlayers[0] == 0)
                {
                    if (redPuppetAnim != null) redPuppetAnim.SetActive(true);
                }
                else if (winningPlayers[0] == 1)
                {
                    if (bluePuppetAnim != null) bluePuppetAnim.SetActive(true);
                }
            }
        }

        HighlightButton();
    }

    /*void Update()
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
    }*/

    void Update()
    {
        if (GameManager.Instance == null) return;

        for (int i = 0; i < GameManager.Instance.playerCount; i++)
        {
            int assignedControllerIndex = GameManager.Instance.controllerAssignments[i];
            if (assignedControllerIndex < 0 || assignedControllerIndex >= Gamepad.all.Count)
                continue; // Skip unassigned or disconnected controllers

            var gamepad = Gamepad.all[assignedControllerIndex];
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
                canMove = true;

            // Confirm selection
            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                menuButtons[currentIndex].onClick.Invoke();
            }
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
        SceneManager.LoadScene(shopSceneName);
    }
}
