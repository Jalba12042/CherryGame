using UnityEngine;
using System.Collections;

public class Cherry : MonoBehaviour
{
    [HideInInspector] public bool ignoreBasketPull = false;

    public IEnumerator TemporarilyIgnoreBasket(float duration)
    {
        ignoreBasketPull = true;
        yield return new WaitForSeconds(duration);
        ignoreBasketPull = false;
    }
}
