using System.Collections;
using UnityEngine;

public class SnowballThrow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInteract playerInteract;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform handHoldPoint;

    [Header("Throw")]
    [SerializeField] private float throwSpeed = 50f;

    private GameObject heldSnowball;

    [SerializeField] private float initialThrowDelay = 0.5f;
    private float throwDelayTimer = 0f;
    private bool firstSnowball = true;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerInteract == null)
            playerInteract = GetComponent<PlayerInteract>();

        if (handHoldPoint == null)
            handHoldPoint = playerInteract.handHoldPoint;
    }

    public void PickUpSnowball(GameObject snowball)
    {
        heldSnowball = snowball;

        if (firstSnowball)
        {
            throwDelayTimer = initialThrowDelay;
            firstSnowball = false;
        }
    }

    private void Update()
    {
        if (throwDelayTimer > 0f)
            throwDelayTimer -= Time.deltaTime;
    }

    public void ThrowSnowball()
    {
        if (heldSnowball == null)
            return;

        if (throwDelayTimer > 0f)
            return;


        if (animator != null)
        {
            animator.SetBool("isAiming", false);
            animator.SetTrigger("doThrow");
        }

        Vector3 throwDirection = transform.forward;
        StartCoroutine(DelayedThrow(throwDirection));
    }

    public void CancelAim()
    {

        if (animator != null)
            animator.SetBool("isAiming", false);
    }

    private IEnumerator DelayedThrow(Vector3 throwDirection)
    {
        yield return new WaitForSeconds(0.05f);

        if (heldSnowball == null)
            yield break;

        GameObject thrownSnowball = heldSnowball;

        // Stop being held
        thrownSnowball.transform.SetParent(null);


        Rigidbody rb = heldSnowball.GetComponent<Rigidbody>();

        if (rb != null)
        {

            Snowball snowball = heldSnowball.GetComponent<Snowball>();

            if (snowball != null)
            {
                snowball.MarkThrown();
            }

            rb.isKinematic = false;

            rb.useGravity = false;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Straight throw
            rb.linearVelocity = handHoldPoint.forward * throwSpeed;

            StartCoroutine(ReenableSnowballCollision(thrownSnowball));

            if (animator != null)
            {
                animator.SetBool("isPickingUp", false);
            }
        }

        LevelPickup pickup = heldSnowball.GetComponent<LevelPickup>();

        if (pickup != null)
        {
            pickup.isHeld = false;
            pickup.playerHolding = null;
        }

        heldSnowball = null;

        playerInteract.NotifyThrowEnded();
        playerInteract.OnSnowballThrown();

        
    }


    private IEnumerator ReenableSnowballCollision(GameObject snowball)
    {
        yield return new WaitForSeconds(0.15f);

        if (snowball == null)
            yield break;

        Collider[] playerColliders =
            GetComponentsInChildren<Collider>();

        Collider[] snowballColliders =
            snowball.GetComponentsInChildren<Collider>();

        foreach (Collider playerCollider in playerColliders)
        {
            foreach (Collider snowballCollider in snowballColliders)
            {
                Physics.IgnoreCollision(
                    playerCollider,
                    snowballCollider,
                    false
                );
            }
        }
    }

    public void ResetFirstSnowball()
    {
        firstSnowball = true;
    }
}