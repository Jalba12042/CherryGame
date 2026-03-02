using Assets.DuckType.Jiggle;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCherry : MonoBehaviour
{
    [Header("Throw Settings")]
    public Transform handHoldPoint;

    [Header("Pickup Audio")]
    [SerializeField] private AudioSource pickupSource;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private AudioClip dropClip;

    private Playermovement player;
    private Gamepad gamepad;
    private Animator animator;
    private Projectile projectileScript;
    private GameObject heldCherry;
    private GameObject nearbyCherry;

    private bool isThrowing = false;

    private Jiggle[] jiggleParts;

    void Start()
    {
        player = GetComponent<Playermovement>();
        animator = GetComponent<Animator>();
        projectileScript = GetComponent<Projectile>();
        jiggleParts = GetComponentsInChildren<Jiggle>();
    }

    /*void Update()
    {
        if (!GameManager.Instance.isOnKeyboard)
        {
            if (player.assignedGamepad == null) return;
            gamepad = player.assignedGamepad;


            float rtValue = gamepad.rightTrigger.ReadValue();

            if (rtValue > 0.1f)
                HandlePickup();
            else
                HandleDrop();
        }
        else
        {
            if (Input.GetKey(KeyCode.E))
            {
                HandlePickup();
            }
            else if (Input.GetKeyUp(KeyCode.E))
            {
                HandleDrop();
            } 
        }
    }*/

    void Update()
    {
        float rtValue = 0f;
        bool isPickingKey = false;

        // ---- INPUT ----
        if (!GameManager.Instance.isOnKeyboard)
        {
            if (player.assignedGamepad == null) return;
            gamepad = player.assignedGamepad;

            rtValue = gamepad.rightTrigger.ReadValue();
            isPickingKey = rtValue > 0.1f;

            // If holding cherry but RT released, cancel aim and drop
            if (heldCherry != null && rtValue <= 0.1f)
            {
                CancelAimAndDrop();
                return; // early exit so we don't pick up again
            }

            if (isPickingKey)
                HandlePickup();
            else
                HandleDrop();
        }
        else
        {
            isPickingKey = Input.GetKey(KeyCode.E);

            // If holding cherry but E released, cancel aim and drop
            if (heldCherry != null && !isPickingKey)
            {
                CancelAimAndDrop();
                return;
            }

            if (isPickingKey)
                HandlePickup();
            else
                HandleDrop();
        }
    }

    private void HandlePickup()
    {
        if (heldCherry == null && nearbyCherry != null)
        {
            heldCherry = nearbyCherry;

            if (pickupSource != null && pickupClip != null)
            {
                pickupSource.pitch = Random.Range(0.95f, 1.05f); // slight variation (optional but nice)
                pickupSource.PlayOneShot(pickupClip);
            }

            SetJiggle(false);

            Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
            if (rbCherry != null) rbCherry.isKinematic = true;

            heldCherry.transform.SetParent(handHoldPoint);
            SetCherryCollision(false);   // disable collisions
            heldCherry.transform.localPosition = Vector3.zero;

            projectileScript?.PickUpCherry(heldCherry);

            if (animator != null)
                StartCoroutine(PlayPickupAnimation());
        }
    }

    private void CancelAimAndDrop()
    {
        if (heldCherry == null) return;

        if (pickupSource != null && dropClip != null)
        {
            pickupSource.pitch = Random.Range(0.9f, 1.0f); // slightly lower pitch for drop
            pickupSource.PlayOneShot(dropClip);
        }

        // Stop aiming in Projectile
        projectileScript?.CancelAim();

        // Drop the cherry
        Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
        heldCherry.transform.SetParent(null);
        if (rbCherry != null)
            rbCherry.isKinematic = false;

        SetCherryCollision(true);
        SetJiggle(true);

        if (animator != null)
            animator.SetBool("isPickingUp", false);

        heldCherry = null;
    }

    private IEnumerator PlayPickupAnimation()
    {
        animator.SetBool("isPickingUp", true);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
    }

    private void HandleDrop()
    {
        // If the Projectile reports aiming or a pending throw, DO NOT drop.
        if (projectileScript != null && (projectileScript.IsThrowPending() || isThrowing))
            return;

        if (heldCherry != null)
        {
            if (pickupSource != null && dropClip != null)
            {
                pickupSource.pitch = Random.Range(0.9f, 1.0f); // slightly lower pitch for drop
                pickupSource.PlayOneShot(dropClip);
            }

            Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
            heldCherry.transform.SetParent(null);
            if (rbCherry != null) rbCherry.isKinematic = false;

            projectileScript?.CancelAim();
            if (animator != null)
                animator.SetBool("isPickingUp", false);

           SetCherryCollision(true);    // re-enable collisions


            SetJiggle(true);
            heldCherry = null;
        }
    }


    /*private void HandleDrop()
    {
        if (projectileScript.IsAiming())
            return;

        if (heldCherry != null)
        {
            Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
            heldCherry.transform.SetParent(null);
            if (rbCherry != null) rbCherry.isKinematic = false;
            projectileScript?.CancelAim();
            if (animator != null)
                animator.SetBool("isPickingUp", false);
            heldCherry = null;
        }
    }*/

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cherry"))
            nearbyCherry = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Cherry") && other.gameObject == nearbyCherry)
            nearbyCherry = null;
    }

    public void NotifyThrowStarted()
    {
        isThrowing = true;
    }

    public void NotifyThrowEnded()
    {
        isThrowing = false;
    }

    void SetCherryCollision(bool enabled)
    {
        if (heldCherry == null) return;

        Collider[] playerCols = GetComponentsInChildren<Collider>();
        Collider[] cherryCols = heldCherry.GetComponentsInChildren<Collider>();

        foreach (var p in playerCols)
        {
            foreach (var c in cherryCols)
            {
                Physics.IgnoreCollision(p, c, !enabled);
            }
        }
    }

    void SetJiggle(bool enabled)
    {
        if (jiggleParts == null) return;

        foreach (var jiggle in jiggleParts)
        {
            jiggle.enabled = enabled;
        }
    }

}
