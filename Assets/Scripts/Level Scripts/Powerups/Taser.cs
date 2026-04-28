using UnityEngine;

public class Taser : Powerup
{
    private Animator playerAnimator;
    private bool isEquipped = false;
    private GameObject owner;
    private bool isDestroyed = false;

    protected override void powerUpEffect()
    {
        base.powerUpEffect();
        //pe.isTasing = true;

        if (activeTimer != null)
        {
            StopCoroutine(activeTimer);
            activeTimer = null;
        }

        if (despawnRoutine != null)
        {
            StopCoroutine(despawnRoutine);
            despawnRoutine = null;
        }
    }

    public void EquipTaser(Transform hand)
    {
        owner = hand.root.gameObject;
        playerAnimator = hand.root.GetComponent<Animator>();

        if (playerAnimator != null)
            playerAnimator.SetBool("isPickingUp", true);

        transform.SetParent(hand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        GetComponent<PowerUpFloat>()?.SetHeld(true);

        isEquipped = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isEquipped) return;

        if (other.gameObject == owner) return;

        if (other.CompareTag("Player"))
        {
            PlayerPowerupHandler handler = other.GetComponent<PlayerPowerupHandler>();

            if (handler != null)
            {
                handler.ApplyTase(2f); // or your stun duration
            }

            OnDestroy();
        }
    }

    private void Cleanup()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if (playerAnimator != null)
            playerAnimator.SetBool("isPickingUp", false);

        transform.SetParent(null);
        GetComponent<PowerUpFloat>()?.SetHeld(false);

        isEquipped = false;
    }

    private void OnDestroy()
    {
        Cleanup();
    }


    protected override void powerUpEnd()
    {
        base.powerUpEnd();
        //pe.isTasing = false;

        if (playerAnimator != null)
            playerAnimator.SetBool("isPickingUp", false);
    }
}
