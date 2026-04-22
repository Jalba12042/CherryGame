using UnityEngine;

public class FaceCamManager : MonoBehaviour
{
    public static FaceCamManager Instance;

    [Header("Face Cam Statics")]
    public FaceCamStatic[] faceCamStatics;

    [Header("Power Up UIs")]
    public PlayerPowerUpUI[] playerUIs;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public FaceCamStatic GetFaceCamStatic(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < faceCamStatics.Length)
            return faceCamStatics[playerIndex];
        return null;
    }

    // --- DEBUG POWER-UP LOGIC ---
    public void ShowPowerUp(int playerID, string powerUpType)
    {
        Debug.Log($"[FaceCamManager] 🚨 REQUEST RECEIVED: Show {powerUpType} for PlayerID {playerID}");

        bool foundUI = false;
        foreach (PlayerPowerUpUI ui in playerUIs)
        {
            if (ui != null && ui.playerID == playerID)
            {
                foundUI = true;
                Debug.Log($"[FaceCamManager] ✅ Found matching UI for Player {playerID}. Telling it to show {powerUpType}!");

                if (powerUpType == "Protein") ui.ShowProtein();
                else if (powerUpType == "Coffee") ui.ShowCoffee();
                else if (powerUpType == "Pill") ui.ShowPill();
                else if (powerUpType == "Taser") ui.ShowTaser();
                else if (powerUpType == "Magnet") ui.ShowMagnet();
                else if (powerUpType == "Gun") ui.ShowGun();
            }
        }

        if (!foundUI)
        {
            Debug.LogError($"[FaceCamManager] ❌ ERROR: Could not find any UI assigned to PlayerID {playerID}!");
        }
    }

    public void HidePowerUp(int playerID)
    {
        foreach (PlayerPowerUpUI ui in playerUIs)
        {
            if (ui != null && ui.playerID == playerID)
            {
                ui.HideIcon();
            }
        }
    }
}