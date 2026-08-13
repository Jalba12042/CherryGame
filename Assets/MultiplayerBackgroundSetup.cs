using UnityEngine;
using UnityEngine.UI;
using System.Linq; // Added this so we can easily sort the buttons

public class MultiplayerBackgroundSetup : MonoBehaviour
{
    [Header("Backgrounds")]
    public Image backgroundImage;
    public Sprite beachBackground;
    public Sprite icebergBackground;

    [Header("Sign Groups")]
    [Tooltip("Drag the parent object holding your 3 real Beach buttons here")]
    public GameObject beachSignGroup;

    [Tooltip("Drag the parent object holding your 3 real Iceberg buttons here")]
    public GameObject icebergSignGroup;

    [Header("Menu Controller Integration")]
    [Tooltip("Drag your MenuManager object here so the script can update its buttons!")]
    public MainMenuController menuController;

    void Start()
    {
        // 0 = Beach, 1 = Iceberg (Matches the dice roll from the Main Menu)
        int bgChoice = PlayerPrefs.GetInt("MultiplayerBG", 0);

        GameObject activeGroup = null;

        if (bgChoice == 0)
        {
            // Setup Beach
            if (backgroundImage != null) backgroundImage.sprite = beachBackground;

            if (beachSignGroup != null) beachSignGroup.SetActive(true);
            if (icebergSignGroup != null) icebergSignGroup.SetActive(false);

            activeGroup = beachSignGroup;
        }
        else
        {
            // Setup Iceberg
            if (backgroundImage != null) backgroundImage.sprite = icebergBackground;

            if (icebergSignGroup != null) icebergSignGroup.SetActive(true);
            if (beachSignGroup != null) beachSignGroup.SetActive(false);

            activeGroup = icebergSignGroup;
        }

        // --- INJECT THE CORRECT BUTTONS ---
        if (menuController != null && activeGroup != null)
        {
            // Automatically grab all the MenuSelectable scripts inside the active folder
            // and sort them based on their order in your Hierarchy (Host -> Join -> Back)
            MenuSelectable[] activeButtons = activeGroup.GetComponentsInChildren<MenuSelectable>()
                                            .OrderBy(b => b.transform.GetSiblingIndex())
                                            .ToArray();

            // Overwrite the MenuController's list with the correct active buttons
            menuController.buttons = activeButtons;

            // Force the controller to highlight the very first button (Host)
            menuController.SelectIndex(0);
        }
    }
}