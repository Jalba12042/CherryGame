using System.Collections;
using UnityEngine;

public class SnowballThrow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInteract playerInteract;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform handHoldPoint;
    [SerializeField] private Playermovement playerMovement;

    [Header("Throw")]
    [SerializeField] private float throwSpeed = 50f;

    private GameObject heldSnowball;
    private GameObject lastThrownSnowball;

    // Lets PlayerInteract ignore collision between this and the next snowball it spawns — the
    // replacement spawns at handHoldPoint in the same frame this one's velocity is set, before
    // physics has had a chance to move it away, so without this the fast just-thrown snowball
    // slams into the new stationary one and its velocity gets killed by the collision.
    public GameObject GetLastThrownSnowball() => lastThrownSnowball;

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

        if (playerMovement == null)
            playerMovement = GetComponent<Playermovement>();
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

        Vector3 throwDirection = playerMovement != null ? playerMovement.GetAimDirection() : transform.forward;

        // Detach from the hand bone right now, before the "doThrow" windup animation gets a
        // chance to swing/dip it — it just stays exactly where it already is (still kinematic,
        // not yet given velocity) instead of continuing to follow the animated bone through the
        // delay below. This avoids teleporting it later, which was leaving the rigidbody in a
        // bad state (floating/drifting instead of a clean launch).
        heldSnowball.transform.SetParent(null);

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


        Rigidbody rb = heldSnowball.GetComponent<Rigidbody>();

        if (rb != null)
        {

            Snowball snowball = heldSnowball.GetComponent<Snowball>();

            if (snowball != null)
            {
                snowball.MarkThrown();
            }

            lastThrownSnowball = thrownSnowball;

            rb.isKinematic = false;

            rb.useGravity = false;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Straight throw — use the direction captured when the throw was pressed, not
            // handHoldPoint.forward read now: the hand bone is mid-swing from the throw
            // animation by this point, so its live forward is animation-timing dependent
            // and produces inconsistent/weird throw angles.
            rb.linearVelocity = throwDirection * throwSpeed;

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