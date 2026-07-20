using Unity.Netcode;
using UnityEngine;

public class Basket : MonoBehaviour
{
    public int numItemsInBasket;

    // Which player (0-3) this basket belongs to, matching its index in BasketContainer.baskets.
    // Set by BasketContainer.Awake(); used to report deposits to StatTracker.
    public int basketOwnerID;

    // Online, only the server's physics simulation is authoritative for scoring - every
    // client's local copy of the cherry also triggers this collider, so without this guard
    // each machine would count independently and disagree.
    private static bool IsAuthority => NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening || NetworkManager.Singleton.IsServer;

    private void Awake()
    {
        numItemsInBasket = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsAuthority) return;
        LevelPickup pickup = other.GetComponent<LevelPickup>();
        if (pickup != null)
        {
            numItemsInBasket += pickup.pointValue;

            // --- THE REPORTER ---
            // Tell the StatTracker that this player just deposited an item!
            if (StatTracker.Instance != null)
            {
                StatTracker.Instance.AddCherry(basketOwnerID);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsAuthority) return;
        LevelPickup pickup = other.GetComponent<LevelPickup>();
        if (pickup != null)
        {
            numItemsInBasket -= pickup.pointValue;

            // Note: If you want players to LOSE a stat point when an item is knocked out, 
            // Max would just add a "RemoveCherry" function in the StatTracker and we'd call it right here.
        }
    }
}