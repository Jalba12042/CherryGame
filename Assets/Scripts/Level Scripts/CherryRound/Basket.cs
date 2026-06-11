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
        Cherry cherry = other.GetComponent<Cherry>();
        if (cherry != null) numItemsInBasket += cherry.pointValue;
    }

    private void OnTriggerExit(Collider other)
    {
        Cherry cherry = other.GetComponent<Cherry>();
        if (cherry != null) numItemsInBasket -= cherry.pointValue;
    }
}
