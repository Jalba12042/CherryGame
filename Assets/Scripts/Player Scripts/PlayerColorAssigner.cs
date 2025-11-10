using UnityEngine;

public class PlayerColorAssigner : MonoBehaviour
{
    [Header("Renderer Reference")]
    public Renderer playerRenderer; // drag your player’s mesh or model renderer here

    [Header("Player Colors")]
    public Color blueColor = Color.blue;
    public Color redColor = Color.red;
    public Color[] playerColors; // optional — use this if you want more than two players

    public void AssignColor(int playerIndex)
    {
        if (playerRenderer == null)
        {
            Debug.LogWarning("No renderer assigned for PlayerColorAssigner!");
            return;
        }

        // If you define a color array, use that — otherwise default to just 2 colors
        if (playerColors != null && playerColors.Length > 0 && playerIndex < playerColors.Length)
        {
            playerRenderer.material.color = playerColors[playerIndex];
        }
        else
        {
            switch (playerIndex)
            {
                case 0:
                    playerRenderer.material.color = blueColor;
                    break;
                case 1:
                    playerRenderer.material.color = redColor;
                    break;
                default:
                    playerRenderer.material.color = Color.white;
                    break;
            }
        }
    }
}
