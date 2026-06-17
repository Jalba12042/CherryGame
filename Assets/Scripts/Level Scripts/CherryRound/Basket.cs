using UnityEngine;

public class Basket : MonoBehaviour
{
    public int numItemsInBasket;

    private void Awake()
    {
        numItemsInBasket = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        LevelPickup pickup = other.GetComponent<LevelPickup>();
        if (pickup != null) numItemsInBasket += pickup.pointValue;
    }

    private void OnTriggerExit(Collider other)
    {
        LevelPickup pickup = other.GetComponent<LevelPickup>();
        if (pickup != null) numItemsInBasket -= pickup.pointValue;
    }
}
