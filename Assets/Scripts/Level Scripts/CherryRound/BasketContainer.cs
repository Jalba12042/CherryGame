using System.Collections.Generic;
using UnityEngine;

public class BasketContainer : MonoBehaviour
{
    public List<GameObject> baskets;

    private void Awake()
    {
        for (int i = 0; i < baskets.Count; i++)
        {
            if (i > GameManager.Instance.playerCount-1)
            {
                baskets[i].SetActive(false);
            }
        }
    }

    public int[] countCherries()
    {
        int[] scores = new int[GameManager.Instance.playerCount];
        for (int i = 0; i < scores.Length; i++)
        {
            Basket b = baskets[i].GetComponent<Basket>();
            scores[i] = b.numItemsInBasket;
        }

        return scores;
    }
}
