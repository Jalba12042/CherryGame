using UnityEngine;

public class PlayerColorAssigner : MonoBehaviour
{
    [Header("Renderer Reference")]
    public Renderer playerRenderer; // drag your player’s mesh or model renderer here

    [Header("Player Materials")]
    public Material[] playerMaterials;

    public void AssignColor(int playerIndex)
    {
        if (playerRenderer == null)
        {
            Debug.LogWarning("No renderer assigned for PlayerColorAssigner!");
            return;
        }

        // If you define a color array, use that — otherwise default to just 2 colors
        if (playerMaterials != null && playerMaterials.Length > 0 && playerIndex < playerMaterials.Length)
        {
            // Assign the whole material (shader + properties)
            playerRenderer.material = playerMaterials[playerIndex];
        }
        else
        {
            Debug.LogWarning($"No material found for player index {playerIndex}");
        }
    }
}
