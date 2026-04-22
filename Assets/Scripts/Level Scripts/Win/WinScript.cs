using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class VictoryPuppet
{
    public string colorName;
    public int colorIndex;
    public GameObject puppetGroup;
}

public class WinScript : MonoBehaviour
{
    public static List<int> winningPlayers = new List<int>();

    [Header("UI Elements")]
    public TextMeshProUGUI winnerText;
    public string shopSceneName = "Shop";

    [Header("Name Mapping")]
    public string[] availableNames;

    [Header("Puppet Animations")]
    public VictoryPuppet[] colorPuppets;

    // --- NEW: This is now a list so you can drop in as many tie animations as you want! ---
    public GameObject[] tiePuppetGroups;

    void Start()
    {
        // 1. Turn off ALL puppets first so the screen is completely clean
        TurnOffAllPuppets();

        // 2. Figure out who won based on the list from RoundManager
        bool isTie = winningPlayers.Count > 1;
        int winnerID = winningPlayers.Count > 0 ? winningPlayers[0] : 0;

        if (isTie)
        {
            // --- IT'S A TIE ---
            if (winnerText != null) winnerText.text = "IT'S A TIE!";

            // --- NEW: Pick a random tie animation from the list! ---
            if (tiePuppetGroups != null && tiePuppetGroups.Length > 0)
            {
                int randomIndex = Random.Range(0, tiePuppetGroups.Length);
                if (tiePuppetGroups[randomIndex] != null)
                {
                    tiePuppetGroups[randomIndex].SetActive(true);
                }
            }
        }
        else
        {
            // --- SOMEONE WON ---
            string winName = "PLAYER " + (winnerID + 1);
            int winColorIndex = 0;

            // 3. Grab their custom data from the GameManager's Memory Bank!
            if (GameManager.Instance != null && GameManager.Instance.playerCustomizations.Count > winnerID)
            {
                var data = GameManager.Instance.playerCustomizations[winnerID];
                winColorIndex = data.colorIndex;

                if (availableNames != null && data.nameIndex >= 0 && data.nameIndex < availableNames.Length)
                {
                    winName = availableNames[data.nameIndex];
                }
            }

            // Set the Name Text on the screen
            if (winnerText != null) winnerText.text = winName + " WINS!";

            // 4. Turn on the correct colored puppet!
            bool foundPuppet = false;
            foreach (VictoryPuppet vp in colorPuppets)
            {
                if (vp.colorIndex == winColorIndex)
                {
                    if (vp.puppetGroup != null) vp.puppetGroup.SetActive(true);
                    foundPuppet = true;
                    break;
                }
            }

            // Fallback: If you forgot to drag a puppet into the inspector, just use the first one
            if (!foundPuppet && colorPuppets.Length > 0 && colorPuppets[0].puppetGroup != null)
            {
                colorPuppets[0].puppetGroup.SetActive(true);
            }
        }
    }

    private void TurnOffAllPuppets()
    {
        // --- NEW: Turn off EVERY tie animation in the list ---
        if (tiePuppetGroups != null)
        {
            foreach (GameObject tieGroup in tiePuppetGroups)
            {
                if (tieGroup != null) tieGroup.SetActive(false);
            }
        }

        foreach (VictoryPuppet vp in colorPuppets)
        {
            if (vp.puppetGroup != null) vp.puppetGroup.SetActive(false);
        }
    }

    public void LoadShop()
    {
        SceneManager.LoadScene(shopSceneName);
    }
}