using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ColoredPowerUpSet
{
    public string colorName;
    public int colorIndex;

    [Header("The Sprites for this Color")]
    public Sprite proteinSprite;
    public Sprite coffeeSprite;
    public Sprite pillSprite;
    public Sprite taserSprite;
    public Sprite magnetSprite;
    public Sprite gunSprite;
}

public class PlayerPowerUpUI : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerID;

    [Header("UI Elements")]
    public Image actionIconImage;

    [Header("Master Sprite List")]
    public ColoredPowerUpSet[] colorSets;

    private int myColorIndex = 0;

    void Start()
    {
        if (actionIconImage != null) actionIconImage.gameObject.SetActive(false);

        // DEBUG: Check Memory Bank
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.playerCustomizations.Count > playerID)
            {
                myColorIndex = GameManager.Instance.playerCustomizations[playerID].colorIndex;
                Debug.Log($"[PlayerPowerUpUI] 🟢 Player {playerID} initialized successfully! Their Color Index is: {myColorIndex}");
            }
            else
            {
                Debug.LogWarning($"[PlayerPowerUpUI] ⚠️ Player {playerID} tried to load, but GameManager doesn't have their data! Count is {GameManager.Instance.playerCustomizations.Count}");
            }
        }
        else
        {
            Debug.LogError($"[PlayerPowerUpUI] ❌ GameManager is completely MISSING!");
        }
    }

    public void ShowProtein() { ShowIconForMyColor("Protein"); }
    public void ShowCoffee() { ShowIconForMyColor("Coffee"); }
    public void ShowPill() { ShowIconForMyColor("Pill"); }
    public void ShowTaser() { ShowIconForMyColor("Taser"); }
    public void ShowMagnet() { ShowIconForMyColor("Magnet"); }
    public void ShowGun() { ShowIconForMyColor("Gun"); }

    public void HideIcon()
    {
        if (actionIconImage != null) actionIconImage.gameObject.SetActive(false);
    }

    private void ShowIconForMyColor(string powerUpType)
    {
        Debug.Log($"[PlayerPowerUpUI] 🔍 Searching for {powerUpType} art for Player {playerID} (Looking for Color Index {myColorIndex})...");

        if (actionIconImage == null)
        {
            Debug.LogError($"[PlayerPowerUpUI] ❌ ERROR: Action Icon Image is not assigned in the Inspector for Player {playerID}!");
            return;
        }

        Sprite spriteToShow = null;
        bool foundColorFolder = false;

        foreach (ColoredPowerUpSet set in colorSets)
        {
            if (set.colorIndex == myColorIndex)
            {
                foundColorFolder = true;
                Debug.Log($"[PlayerPowerUpUI] ✅ Found the matching color folder: {set.colorName}!");

                if (powerUpType == "Protein") spriteToShow = set.proteinSprite;
                else if (powerUpType == "Coffee") spriteToShow = set.coffeeSprite;
                else if (powerUpType == "Pill") spriteToShow = set.pillSprite;
                else if (powerUpType == "Taser") spriteToShow = set.taserSprite;
                else if (powerUpType == "Magnet") spriteToShow = set.magnetSprite;
                else if (powerUpType == "Gun") spriteToShow = set.gunSprite;
                break;
            }
        }

        if (!foundColorFolder)
        {
            Debug.LogError($"[PlayerPowerUpUI] ❌ ERROR: Could not find a color folder with Index {myColorIndex}!");
        }
        else if (spriteToShow == null)
        {
            Debug.LogError($"[PlayerPowerUpUI] ❌ ERROR: Found the folder, but the {powerUpType} Sprite slot is EMPTY in the Inspector!");
        }
        else
        {
            Debug.Log($"[PlayerPowerUpUI] 🎉 SUCCESS! Applying {powerUpType} sprite and turning image ON!");
            actionIconImage.sprite = spriteToShow;
            actionIconImage.gameObject.SetActive(true);
        }
    }
}