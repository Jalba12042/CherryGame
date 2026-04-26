using UnityEngine;
using System.Collections;

public class Cherry : MonoBehaviour
{
    [HideInInspector] public bool ignoreBasketPull = false;
    public bool isHeld = false;
    public GameObject playerHolding;

    private GroundCheck groundCheck;
    private bool wasGrounded = false;

    [Header("Trail")]
    [SerializeField] private GameObject cherryTrailObject;

    void Awake()
    {
        groundCheck = GetComponent<GroundCheck>();

        if (cherryTrailObject != null)
            cherryTrailObject.SetActive(false); // OFF by default
    }

    void Update()
    {
        if (groundCheck == null) return;

        // Detect landing moment (NOT just being grounded)
        if (groundCheck.isGrounded && !wasGrounded)
        {
            DisableTrail();
        }

        wasGrounded = groundCheck.isGrounded;
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
