using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ScoreboardRow
{
    public GameObject rowParent;
    public Image headIconImage;
    public TMP_Text playerNameText;
    public GameObject[] tallyMarks;
}

public class ScoreboardUI : MonoBehaviour
{
    [Header("Player Rows")]
    public ScoreboardRow[] playerRows = new ScoreboardRow[4];

    [Header("Color Data Database")]
    public ShopColorData[] availableColors;
    public string[] availableNames;

    void Start()
    {
        UpdateScoreboard();
    }

    public void UpdateScoreboard()
    {
        int safePlayerCount = 4;
        if (GameManager.Instance != null) safePlayerCount = Mathf.Min(GameManager.Instance.playerCount, 4);

        for (int pIndex = 0; pIndex < 4; pIndex++)
        {
            // --- HIDE EMPTY PLAYERS ---
            if (pIndex >= safePlayerCount)
            {
                if (playerRows[pIndex].headIconImage != null) playerRows[pIndex].headIconImage.gameObject.SetActive(false);
                if (playerRows[pIndex].playerNameText != null) playerRows[pIndex].playerNameText.gameObject.SetActive(false);

                foreach (var tally in playerRows[pIndex].tallyMarks)
                {
                    if (tally != null) tally.SetActive(false);
                }
                continue;
            }

            // --- PLAYER IS ACTIVE ---
            var pData = GameManager.Instance.playerCustomizations[pIndex];

            // Set Name
            if (playerRows[pIndex].playerNameText != null && pData.nameIndex >= 0 && pData.nameIndex < availableNames.Length)
            {
                playerRows[pIndex].playerNameText.text = availableNames[pData.nameIndex];
                playerRows[pIndex].playerNameText.gameObject.SetActive(true);
            }

            // Get Color Data
            ShopColorData colorInfo = GetColorData(pData.colorIndex);
            Sprite playerTallySprite = null; // Will hold your custom colored tally mark!

            if (colorInfo != null)
            {
                playerTallySprite = colorInfo.tallyMarkSprite; // Grab the pre-drawn sprite!

                if (playerRows[pIndex].headIconImage != null)
                {
                    playerRows[pIndex].headIconImage.sprite = colorInfo.headIcon;
                    playerRows[pIndex].headIconImage.gameObject.SetActive(true);
                }
            }

            // DRAW THE CUSTOM TALLIES!
            int wins = GameManager.Instance.playerTotalScores[pIndex];

            for (int i = 0; i < playerRows[pIndex].tallyMarks.Length; i++)
            {
                if (playerRows[pIndex].tallyMarks[i] != null)
                {
                    bool showTally = i < wins;
                    playerRows[pIndex].tallyMarks[i].SetActive(showTally);

                    // If the tally is showing, swap out the picture!
                    if (showTally && playerTallySprite != null)
                    {
                        Image tallyImage = playerRows[pIndex].tallyMarks[i].GetComponent<Image>();
                        if (tallyImage != null)
                        {
                            tallyImage.sprite = playerTallySprite; // Slap the Pink/Cyan/Green image on it!
                            tallyImage.color = Color.white; // Force color to white so Unity doesn't tint your drawing
                        }
                    }
                }
            }
        }
    }

    private ShopColorData GetColorData(int colorIndex)
    {
        foreach (var color in availableColors)
        {
            if (color.colorIndex == colorIndex) return color;
        }
        return availableColors.Length > 0 ? availableColors[0] : null;
    }
}