using UnityEngine;
using System.Collections;

public class Cherry : LevelPickup
{
    [HideInInspector] public bool ignoreBasketPull = false;
    public int pointValue = 1;

    public IEnumerator TemporarilyIgnoreBasket(float duration)
    {
        ignoreBasketPull = true;
        yield return new WaitForSeconds(duration);
        ignoreBasketPull = false;
    }
}
