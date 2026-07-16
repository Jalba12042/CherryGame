using UnityEngine;

public class StatTracker : MonoBehaviour
{
    public static StatTracker Instance;

    [Header("The Vault: Cumulative Stats")]
    // Arrays set to hold stats for 4 players max
    public int[] totalPistolKills = new int[4];
    public int[] totalCherriesDeposited = new int[4];

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // This is crucial: It keeps the stats alive when the scene reloads!
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ==========================================
    // --- THE REPORTER FUNCTIONS ---
    // ==========================================

    public void AddPistolKill(int playerID)
    {
        if (playerID >= 0 && playerID < 4)
        {
            totalPistolKills[playerID]++;
            Debug.Log("Player " + (playerID + 1) + " got a pistol kill! Total: " + totalPistolKills[playerID]);
        }
    }

    public void AddCherry(int playerID)
    {
        if (playerID >= 0 && playerID < 4)
        {
            totalCherriesDeposited[playerID]++;
            Debug.Log("Player " + (playerID + 1) + " deposited a cherry! Total: " + totalCherriesDeposited[playerID]);
        }
    }
}
