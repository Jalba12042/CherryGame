using System.Collections.Generic;
using UnityEngine;

public class BasketContainer : MonoBehaviour
{
    public List<GameObject> baskets;

    // Online play sets this to the real total across all clients (RoundManager.playerCount is
    // only this machine's local count, and doesn't know about other clients' players yet at
    // Awake() time). -1 means "use GameManager.Instance.playerCount" (local play).
    [HideInInspector] public int onlinePlayerCountOverride = -1;

    private int EffectivePlayerCount => onlinePlayerCountOverride >= 0 ? onlinePlayerCountOverride : GameManager.Instance.playerCount;

    private void Awake()
    {
        for (int i = 0; i < baskets.Count; i++)
        {
            Basket basket = baskets[i].GetComponentInChildren<Basket>();
            if (basket != null) basket.basketOwnerID = i;

            if (i > GameManager.Instance.playerCount-1)
            {
                baskets[i].SetActive(false);
            }
        }
    }

    // count cherries in baskets
    public int[] countCherries()
    {
        int[] scores = new int[EffectivePlayerCount];

        for (int i = 0; i < scores.Length; i++)
        {
            Basket b = baskets[i].GetComponentInChildren<Basket>();
            if (b != null)
                scores[i] = b.numItemsInBasket;
            else
                Debug.LogError($"Basket script missing on basket {i} (GameObject: {baskets[i].name})");
        }

        return scores;
    }


    #region Maxs old countCherries()
    /*public int[] countCherries()
    {
        int[] scores = new int[GameManager.Instance.playerCount];
        for (int i = 0; i < scores.Length; i++)
        {
            Basket b = baskets[i].GetComponent<Basket>();
            scores[i] = b.numItemsInBasket;
        }

        return scores;
    }*/
    #endregion
}
