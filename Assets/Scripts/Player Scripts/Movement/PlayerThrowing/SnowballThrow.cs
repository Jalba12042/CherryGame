using System.Collections;
using UnityEngine;

public class SnowballThrow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInteract playerInteract;
    [SerializeField] private Animator animator;

    [Header("Throw")]
    [SerializeField] private float throwSpeed = 50f;

    private GameObject heldSnowball;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerInteract == null)
            playerInteract = GetComponent<PlayerInteract>();
    }

    public void PickUpSnowball(GameObject snowball)
    {
        heldSnowball = snowball;
    }

    public void ThrowSnowball()
    {
        if (heldSnowball == null)
            return;

        if (animator != null)
        {
            animator.SetBool("isAiming", false);
            animator.SetTrigger("doThrow");
        }

        StartCoroutine(DelayedThrow());
    }

    public void CancelAim()
    {

        if (animator != null)
            animator.SetBool("isAiming", false);
    }

    private IEnumerator DelayedThrow()
    {
        yield return new WaitForSeconds(0.25f);

        if (heldSnowball == null)
            yield break;

        heldSnowball.transform.SetParent(null);

        Rigidbody rb = heldSnowball.GetComponent<Rigidbody>();

        if (rb != null)
        {

            Snowball snowball = heldSnowball.GetComponent<Snowball>();

            if (snowball != null)
            {
                snowball.MarkThrown();
            }

            rb.isKinematic = false;


            // Straight throw
            rb.linearVelocity = transform.forward * throwSpeed;

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

        GameObject thrownSnowball = heldSnowball;

        heldSnowball = null;

        playerInteract.NotifyThrowEnded();
        playerInteract.OnSnowballThrown();

        
    }
}