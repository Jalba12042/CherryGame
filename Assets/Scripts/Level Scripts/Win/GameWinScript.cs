using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class FinalVictoryPuppet
{
    public string colorName;
    public int colorIndex;
    public GameObject puppetGroup;
}

public class GameWinScript : MonoBehaviour
{
    public TMP_Text winnerText; // assign in Inspector to display winner
    public Button[] menuButtons; // Shop button etc.
    private int currentIndex = 0;

    private bool canMove = true;
    private float deadzone = 0.5f;

    [SerializeField] private string localSceneName;

    public static List<int> winningPlayers = new List<int>();

    [Header("Name Mapping")]
    [Tooltip("Copy the exact same list of names from your Customization UI here!")]
    public string[] availableNames;

    [Header("Victory Animations")]
    public FinalVictoryPuppet[] colorPuppets;

    void Start()
    {
        TurnOffAllPuppets();

        // Safety fallback just in case the list is empty
        if (winningPlayers == null || winningPlayers.Count == 0)
        {
            winningPlayers = new List<int> { 0 };
        }

        bool isTie = winningPlayers.Count > 1;

        if (isTie)
        {
            // --- IT'S A TIE ---
            string winnersString = "";
            for (int i = 0; i < winningPlayers.Count; i++)
            {
                int pID = winningPlayers[i];
                string pName = "Player " + (pID + 1);

                // Check Memory Bank for custom name
                if (GameManager.Instance != null && GameManager.Instance.playerCustomizations.Count > pID)
                {
                    int nameIdx = GameManager.Instance.playerCustomizations[pID].nameIndex;
                    if (availableNames != null && nameIdx >= 0 && nameIdx < availableNames.Length)
                    {
                        pName = availableNames[nameIdx];
                    }
                }

                winnersString += pName;
                if (i == winningPlayers.Count - 2) winnersString += " & ";
                else if (i < winningPlayers.Count - 1) winnersString += ", ";
            }
            winnersString += " TIED!";

            if (winnerText != null) winnerText.text = winnersString.ToUpper();
        }
        else
        {
            // --- SOMEONE WON ---
            int winnerID = winningPlayers[0];
            string winName = "PLAYER " + (winnerID + 1);
            int winColorIndex = 0;

            // Grab their custom data from the GameManager
            if (GameManager.Instance != null && GameManager.Instance.playerCustomizations.Count > winnerID)
            {
                var data = GameManager.Instance.playerCustomizations[winnerID];
                winColorIndex = data.colorIndex;

                if (availableNames != null && data.nameIndex >= 0 && data.nameIndex < availableNames.Length)
                {
                    winName = availableNames[data.nameIndex];
                }
            }

            if (winnerText != null) winnerText.text = winName.ToUpper() + " WINS THE GAME!";

            // Turn on correct color puppet
            bool foundPuppet = false;
            if (colorPuppets != null)
            {
                foreach (FinalVictoryPuppet vp in colorPuppets)
                {
                    if (vp.colorIndex == winColorIndex)
                    {
                        if (vp.puppetGroup != null) vp.puppetGroup.SetActive(true);
                        foundPuppet = true;
                        break;
                    }
                }
            }

            // Fallback: If no puppet matches, use the first one
            if (!foundPuppet && colorPuppets != null && colorPuppets.Length > 0 && colorPuppets[0].puppetGroup != null)
            {
                colorPuppets[0].puppetGroup.SetActive(true);
            }
        }

        HighlightButton();
    }

    private void TurnOffAllPuppets()
    {
        if (colorPuppets != null)
        {
            foreach (FinalVictoryPuppet vp in colorPuppets)
            {
                if (vp.puppetGroup != null) vp.puppetGroup.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (Gamepad.all.Count == 0 || menuButtons == null || menuButtons.Length == 0) return;
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
        if (menuButtons == null) return;
        for (int i = 0; i < menuButtons.Length; i++)
        {
            ColorBlock colors = menuButtons[i].colors;
            colors.normalColor = (i == currentIndex) ? Color.yellow : Color.white;
            menuButtons[i].colors = colors;
        }
    }

    public void GoToLocal()
    {
        SceneManager.LoadScene(localSceneName);
    }
}