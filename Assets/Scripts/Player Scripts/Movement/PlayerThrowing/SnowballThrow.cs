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

    private bool isAiming;
    private bool pendingThrow;
    private bool ignoreFirstRelease;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerInteract == null)
            playerInteract = GetComponent<PlayerInteract>();
    }

    public bool IsAiming()
    {
        return isAiming;
    }

    public void PickUpSnowball(GameObject snowball)
    {
        heldSnowball = snowball;

        ignoreFirstRelease = true;
        isAiming = false;

        if (animator != null)
            animator.SetBool("isAiming", false);
    }

    public void CancelAim()
    {
        isAiming = false;

        if (animator != null)
            animator.SetBool("isAiming", false);
    }

    private void Update()
    {
        if (heldSnowball == null)
            return;

        bool rtHeld = InputManager.Instance.GetButton1Held(playerInteract.GetComponent<Playermovement>().playerID);

        // Hold RT
        if (rtHeld)
        {
            isAiming = true;

            if (animator != null)
                animator.SetBool("isAiming", true);
        }

        // Release RT
        // Release RT
        else if (isAiming)
        {
            if (ignoreFirstRelease)
            {
                ignoreFirstRelease = false;
                isAiming = false;

                if (animator != null)
                    animator.SetBool("isAiming", false);

                return;
            }

            isAiming = false;

            if (animator != null)
            {
                animator.SetBool("isAiming", false);
                animator.SetTrigger("doThrow");
            }

            StartCoroutine(DelayedThrow());
        }
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

        playerInteract.NotifyThrowEnded();

        heldSnowball = null;
        pendingThrow = false;
    }
}