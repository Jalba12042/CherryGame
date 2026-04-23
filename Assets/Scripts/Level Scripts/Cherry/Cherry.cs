using UnityEngine;
using System.Collections;

public class Cherry : MonoBehaviour
{
    [HideInInspector] public bool ignoreBasketPull = false;
    public bool isHeld = false;
    public GameObject playerHolding;

    [Header("Trail")]
    [SerializeField] private GameObject cherryTrailObject;

    void Awake()
    {
        if (cherryTrailObject != null)
            cherryTrailObject.SetActive(false); // OFF by default
    }

    public void EnableTrail()
    {
        if (cherryTrailObject != null)
            cherryTrailObject.SetActive(true);
    }

    public void DisableTrail()
    {
        if (cherryTrailObject != null)
            cherryTrailObject.SetActive(false);
    }
    public IEnumerator TemporarilyIgnoreBasket(float duration)
    {
        ignoreBasketPull = true;
        yield return new WaitForSeconds(duration);
        ignoreBasketPull = false;
    }
}
