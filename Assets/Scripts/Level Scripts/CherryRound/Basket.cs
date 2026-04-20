using System.Collections.Generic;
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
        if (other.gameObject.tag == "Cherry")
        {
            numItemsInBasket++;
        }
        if (other.gameObject.tag == "GoldenCherry")
        {
            numItemsInBasket += 3;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Cherry")
        {
            numItemsInBasket--;
        }
        if (other.gameObject.tag == "GoldenCherry")
        {
            numItemsInBasket -= 3;
        }
    }
}
