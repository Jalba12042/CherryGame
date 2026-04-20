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

    [SerializeField] private LayerMask cherryLayer;

    private Playermovement player;
    private Gamepad gamepad;
    private Animator animator;
    private Projectile projectileScript;
    private GameObject heldCherry;
    //private GameObject nearbyCherry;

    private bool isThrowing = false;
    private bool wasPickingLastFrame = false;

    private Jiggle[] jiggleParts;

    [SerializeField] private float pickupRadius = 50f;

    void Start()
    {
        player = GetComponent<Playermovement>();
        animator = GetComponent<Animator>();
        projectileScript = GetComponent<Projectile>();
        jiggleParts = GetComponentsInChildren<Jiggle>();
    }


    void Update()
    {
        bool isPickingNow = false;

        // ---- INPUT ----
        if (!GameManager.Instance.isOnKeyboard)
        {
            if (player.assignedGamepad == null) return;
            gamepad = player.assignedGamepad;

            float rtValue = gamepad.rightTrigger.ReadValue();
            isPickingNow = rtValue > 0.1f;
        }
        else
        {
            isPickingNow = Input.GetKey(KeyCode.E);
        }

        // ---- DETECT PRESS ----
        if (isPickingNow && !wasPickingLastFrame)
        {
            OnPickupPressed();
        }

        // Save for next frame
        wasPickingLastFrame = isPickingNow;
    }

    private void HandlePickup()
    {
        if (heldCherry != null) return;

        //Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius);

        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius, cherryLayer);

        GameObject closestCherry = null;
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Cherry")) continue;

            float dist = Vector3.Distance(transform.position, hit.ClosestPoint(transform.position));
            if (dist < closestDist)
            {
                closestDist = dist;
                closestCherry = hit.gameObject;
            }
        }

        if (closestCherry == null) return;

        heldCherry = closestCherry;

        if (pickupSource != null && pickupClip != null)
        {
            pickupSource.pitch = Random.Range(0.95f, 1.05f);
            pickupSource.PlayOneShot(pickupClip);
        }

        SetJiggle(false);

        Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
        if (rbCherry != null) rbCherry.isKinematic = true;

        heldCherry.transform.SetParent(handHoldPoint);
        SetCherryCollision(false);
        heldCherry.transform.localPosition = Vector3.zero;

        projectileScript?.PickUpCherry(heldCherry);

        if (animator != null)
            StartCoroutine(PlayPickupAnimation());
    }

    private void OnPickupPressed()
    {
        if (heldCherry == null)
        {
            PlayerGrab grab = GetComponent<PlayerGrab>();

            if (grab != null && grab.TryGrab())
            {
                return;
            }

            HandlePickup();
        }
        else
        {
            CancelAimAndDrop();
        }
    }
    public void CancelAimAndDrop()
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

        Cherry cherryScript = heldCherry.GetComponent<Cherry>();
        if (cherryScript != null)
        {
            cherryScript.isHeld = false;
        }

        heldCherry = null;
    }

    private IEnumerator PlayPickupAnimation()
    {
        animator.SetBool("isPickingUp", true);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
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
