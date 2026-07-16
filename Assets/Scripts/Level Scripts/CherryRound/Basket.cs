using UnityEngine;

public class Basket : MonoBehaviour
{
    public int numItemsInBasket;

    [Header("Stat Tracker Info")]
    public int basketOwnerID; // 0 = Player 1, 1 = Player 2, 2 = Player 3, 3 = Player 4

    private void Awake()
    {
        numItemsInBasket = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
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
        LevelPickup pickup = other.GetComponent<LevelPickup>();
        if (pickup != null)
        {
            numItemsInBasket -= pickup.pointValue;

            // Note: If you want players to LOSE a stat point when an item is knocked out, 
            // Max would just add a "RemoveCherry" function in the StatTracker and we'd call it right here.
        }
    }
}