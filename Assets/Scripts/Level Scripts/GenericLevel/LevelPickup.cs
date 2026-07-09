using UnityEngine;
using System.Collections;

public class LevelPickup : MonoBehaviour
{
    [HideInInspector] public bool isHeld = false;
    [HideInInspector] public GameObject playerHolding;

    [Header("Trail")]
    [SerializeField] private GameObject trailObject;

    public bool useProjectileThrow = false;

    private GroundCheck groundCheck;
    private bool wasGrounded = false;

    [HideInInspector] public bool ignoreBasketPull = false;

    public int pointValue = 1;
    public IEnumerator TemporarilyIgnoreBasket(float duration)
    {
        ignoreBasketPull = true;
        yield return new WaitForSeconds(duration);
        ignoreBasketPull = false;
    }

    protected virtual void Awake()
    {
        groundCheck = GetComponent<GroundCheck>();
        if (trailObject != null) trailObject.SetActive(false);
    }

    protected virtual void Update()
    {
        if (groundCheck == null) return;
        if (groundCheck.isGrounded && !wasGrounded)
            DisableTrail();
        wasGrounded = groundCheck.isGrounded;
    }

    public void EnableTrail()
    {
        if (trailObject != null) trailObject.SetActive(true);
    }

    public void DisableTrail()
    {
        if (trailObject != null) trailObject.SetActive(false);
    }
}
