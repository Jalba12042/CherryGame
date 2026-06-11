using UnityEngine;

public class LevelPickup : MonoBehaviour
{
    [HideInInspector] public bool isHeld = false;
    [HideInInspector] public GameObject playerHolding;

    [Header("Trail")]
    [SerializeField] private GameObject trailObject;

    private GroundCheck groundCheck;
    private bool wasGrounded = false;

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
