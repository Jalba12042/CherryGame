using Unity.Netcode;
using UnityEngine;

public class Basket : MonoBehaviour
{
    public int numItemsInBasket;

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
        if (pickup != null) numItemsInBasket += pickup.pointValue;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsAuthority) return;
        LevelPickup pickup = other.GetComponent<LevelPickup>();
        if (pickup != null) numItemsInBasket -= pickup.pointValue;
    }
}
