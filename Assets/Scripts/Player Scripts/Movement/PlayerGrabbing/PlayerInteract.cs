using Assets.DuckType.Jiggle;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Merged from: PlayerGrab, PlayerCherry, PlayerGrabbed
// One RT press attempts, in priority order: release, grab player, pick up cherry.
// LT release throws a held player. Cherry throw is handled by Projectile (hold/release LT).
public class PlayerInteract : MonoBehaviour
{
    // ===== GRAB =====
    [Header("Grab Settings")]
    public Transform pickupTarget;
    public float pickupRange = 2f;
    public float grabCooldownTime = 1f;
    public float escapeCooldownTime = 1f;

    [Header("Throw (Player)")]
    [SerializeField] private float throwForceForward = 25f;
    [SerializeField] private float throwForceUp = 12f;

    [Header("Grab Audio")]
    [SerializeField] private AudioSource grabSource;
    [SerializeField] private AudioClip grabClip;
    [SerializeField] private AudioClip grabDropClip;

    [Header("Stun UI")]
    public GameObject stunCanvas;
    public TMPro.TMP_Text stunTimerText;

    [Header("Face Animator")]
    [SerializeField] private Animator faceAnimator;

    // ===== CHERRY =====
    [Header("Cherry Pickup")]
    public Transform handHoldPoint;
    [SerializeField] private LayerMask cherryLayer;
    [SerializeField] private float pickupRadius = 3f;

    [Header("Cherry Audio")]
    [SerializeField] private AudioSource pickupSource;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private AudioClip cherryDropClip;

    // ===== GRABBED STATE (was PlayerGrabbed) =====
    [HideInInspector] public bool isGrabbed = false;
    [HideInInspector] public PlayerInteract grabber;

    // ===== PRIVATE =====
    private Playermovement player;
    private Projectile projectileScript;
    private Animator animator;
    private Jiggle[] jiggleParts;
    private PlayerPowerupHandler powerupHandler;
    private Collider myCollider;

    private bool canGrab = true;
    private bool grabEscapeCooldown = false;
    private GameObject grabbedPlayer;
    private Rigidbody grabbedRigidbody;
    private Collider grabbedCollider;
    private GameObject nearbyPlayer;
    private bool ltWasHeld = false;


    private bool rtHeld = false;
    private float rtHoldTime = 0f;

    [SerializeField]
    private float throwHoldThreshold = 0.2f;

    private GameObject heldCherry;
    private float cherryPickupCooldown = 0f;

    private void Awake()
    {
        if (faceAnimator == null)
            faceAnimator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        player = GetComponent<Playermovement>();
        powerupHandler = GetComponent<PlayerPowerupHandler>();
        projectileScript = GetComponent<Projectile>();
        animator = GetComponent<Animator>();
        myCollider = GetComponent<Collider>();
        jiggleParts = GetComponentsInChildren<Jiggle>();

        if (stunCanvas != null)
            stunCanvas.SetActive(false);
    }

    void Update()
    {
        if (cherryPickupCooldown > 0f)
            cherryPickupCooldown -= Time.deltaTime;

        if (grabbedPlayer != null && pickupTarget != null)
        {
            grabbedPlayer.transform.position = pickupTarget.position;
            grabbedPlayer.transform.rotation = pickupTarget.rotation;
        }

        if (grabEscapeCooldown) return;

        /*bool rtAvailable = powerupHandler == null || !powerupHandler.rtConsumedThisFrame;
        if (InputManager.Instance.GetGrabDown(player.playerID) && rtAvailable)
            OnInteractPressed();*/

        bool rt = InputManager.Instance.GetButton1Held(player.playerID);

        if (rt)
        {
            rtHeld = true;
            rtHoldTime += Time.deltaTime;
        }
        else if (rtHeld)
        {
            bool wasHold = rtHoldTime >= throwHoldThreshold;

            if (wasHold)
            {
                if (grabbedPlayer != null)
                {
                    ThrowGrabbedPlayer();
                }
            }
            else
            {
                OnInteractPressed();
            }

            rtHeld = false;
            rtHoldTime = 0f;
        }
    }

    private void OnInteractPressed()
    {
        if (grabbedPlayer != null)
        {
            ReleaseGrab();
            return;
        }
        if (heldCherry != null) 
        {
            if (!projectileScript.IsAiming())
            {
                CancelAimAndDrop();
            }

            return;
        }
        if (TryGrab()) return;
        HandlePickup();
    }

    // ===== GRAB =====

    public bool TryGrab()
    {
        if (!canGrab || grabbedPlayer != null || isGrabbed) return false;

        if (nearbyPlayer == null || Vector3.Distance(transform.position, nearbyPlayer.transform.position) > pickupRange)
        {
            nearbyPlayer = null;
            Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange);
            float closest = float.MaxValue;
            foreach (var hit in hits)
            {
                Playermovement pm = hit.GetComponent<Playermovement>() ?? hit.GetComponentInParent<Playermovement>();
                if (pm == null || pm.playerIndex == player.playerIndex) continue;
                float dist = Vector3.Distance(transform.position, pm.transform.position);
                if (dist < closest) { closest = dist; nearbyPlayer = pm.gameObject; }
            }
        }

        if (nearbyPlayer == null) return false;

        PlayerEffects pe = nearbyPlayer.GetComponent<PlayerEffects>();
        if (pe != null && pe.isBig) return false;

        PlayerInteract targetInteract = nearbyPlayer.GetComponent<PlayerInteract>();
        if (targetInteract != null && targetInteract.isGrabbed) return false;

        grabbedPlayer = nearbyPlayer;
        animator.SetBool("isGrabbing", true);

        if (grabSource != null && grabClip != null)
        {
            grabSource.pitch = Random.Range(0.95f, 1.05f);
            grabSource.PlayOneShot(grabClip);
        }

        grabbedRigidbody = grabbedPlayer.GetComponent<Rigidbody>();
        if (grabbedRigidbody != null) grabbedRigidbody.isKinematic = true;

        grabbedCollider = grabbedPlayer.GetComponent<Collider>();
        if (grabbedCollider != null) Physics.IgnoreCollision(myCollider, grabbedCollider, true);

        Playermovement grabbedMovement = grabbedPlayer.GetComponent<Playermovement>();
        if (grabbedMovement != null) grabbedMovement.canMove = false;

        if (targetInteract != null)
        {
            targetInteract.isGrabbed = true;
            targetInteract.grabber = this;
        }

        Animator grabbedAnimator = grabbedPlayer.GetComponent<Animator>();
        if (grabbedAnimator != null) grabbedAnimator.SetBool("isGrabbed", true);

        PlayerEscapeUI escapeUI = grabbedPlayer.GetComponent<PlayerEscapeUI>();
        if (escapeUI != null) escapeUI.StartBeingGrabbed(this);

        SetJiggle(false);
        return true;
    }

    public void ReleaseGrab()
    {
        if (grabbedPlayer == null) return;

        if (grabSource != null && grabDropClip != null)
        {
            grabSource.pitch = Random.Range(0.9f, 1.0f);
            grabSource.PlayOneShot(grabDropClip);
        }

        if (grabbedRigidbody != null) grabbedRigidbody.isKinematic = false;
        if (grabbedCollider != null) Physics.IgnoreCollision(myCollider, grabbedCollider, false);

        Playermovement pm = grabbedPlayer.GetComponent<Playermovement>();
        if (pm != null) pm.canMove = true;

        PlayerEscapeUI escapeUI = grabbedPlayer.GetComponent<PlayerEscapeUI>();
        if (escapeUI != null) escapeUI.StopBeingGrabbed();

        PlayerInteract grabbedInteract = grabbedPlayer.GetComponent<PlayerInteract>();
        if (grabbedInteract != null)
        {
            grabbedInteract.isGrabbed = false;
            grabbedInteract.grabber = null;
            grabbedInteract.enabled = true;
        }

        Animator grabbedAnimator = grabbedPlayer.GetComponent<Animator>();
        if (grabbedAnimator != null) grabbedAnimator.SetBool("isGrabbed", false);

        if (animator != null) animator.SetBool("isGrabbing", false);

        SetJiggle(true);
        grabbedPlayer = null;
        grabbedRigidbody = null;
        grabbedCollider = null;

        StartCoroutine(GrabCooldown());
    }

    private void ThrowGrabbedPlayer()
    {
        if (grabbedPlayer == null || grabbedRigidbody == null) return;

        GameObject thrownPlayer = grabbedPlayer;
        Rigidbody thrownRb = grabbedRigidbody;
        PlayerInteract thrownInteract = grabbedPlayer.GetComponent<PlayerInteract>();
        Playermovement thrownPm = grabbedPlayer.GetComponent<Playermovement>();

        ReleaseGrab();

        if (animator != null) animator.SetBool("isPickingUp", false);

        Animator thrownAnimator = thrownPlayer.GetComponent<Animator>();
        if (thrownAnimator != null) thrownAnimator.SetBool("isGrabbed", false);

        thrownRb.isKinematic = false;
        thrownRb.linearVelocity = Vector3.zero;
        thrownRb.AddForce(transform.forward * throwForceForward + Vector3.up * throwForceUp, ForceMode.Impulse);

        if (thrownPm != null && thrownInteract != null)
            StartCoroutine(thrownInteract.StunRoutine(3f));

        if (thrownInteract != null) thrownInteract.enabled = true;
    }

    public IEnumerator StunRoutine(float duration)
    {
        player.canMove = false;
        animator.SetBool("isStunned", true);

        RandomFaceChanger rfc = GetComponentInChildren<RandomFaceChanger>();
        if (rfc != null) rfc.PauseFaces();
        if (faceAnimator != null) faceAnimator.SetBool("isStunned", true);
        if (stunCanvas != null) stunCanvas.SetActive(true);

        float remaining = duration;
        while (remaining > 0f)
        {
            if (stunTimerText != null)
                stunTimerText.text = Mathf.Ceil(remaining).ToString();
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }

        if (faceAnimator != null) faceAnimator.SetBool("isStunned", false);
        if (rfc != null) rfc.ResumeFaces();
        if (stunCanvas != null) stunCanvas.SetActive(false);

        player.canMove = true;
        animator.SetBool("isStunned", false);
    }

    public IEnumerator GrabCooldown()
    {
        canGrab = false;
        yield return new WaitForSeconds(grabCooldownTime);
        canGrab = true;
    }

    public void ForceRelease()
    {
        ReleaseGrab();
        StartCoroutine(EscapeGrabCooldown());
    }

    // Replaces PlayerGrabbed.ReleaseGrabbedPlayer()
    public void ReleaseIfGrabbed()
    {
        if (!isGrabbed || grabber == null) return;
        grabber.ReleaseGrab();
        isGrabbed = false;
    }

    public void ForceReleaseAll()
    {
        if (grabbedPlayer != null) ReleaseGrab();
        if (isGrabbed) ReleaseIfGrabbed();
    }

    public void ResetGrabState()
    {
        isGrabbed = false;
        grabber = null;
        grabEscapeCooldown = false;
        canGrab = true;
        nearbyPlayer = null;
        if (grabbedPlayer != null) ReleaseGrab();
    }

    private IEnumerator EscapeGrabCooldown()
    {
        grabEscapeCooldown = true;
        yield return new WaitForSeconds(escapeCooldownTime);
        grabEscapeCooldown = false;
    }

    // ===== CHERRY =====

    private void HandlePickup()
    {
        if (heldCherry != null || cherryPickupCooldown > 0f) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius, cherryLayer);
        GameObject closestCherry = null;
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Cherry") && !hit.CompareTag("GoldenCherry")) continue;
            float dist = Vector3.Distance(transform.position, hit.ClosestPoint(transform.position));
            if (dist < closestDist) { closestDist = dist; closestCherry = hit.gameObject; }
        }

        if (closestCherry == null) return;

        heldCherry = closestCherry;

        Cherry cherryScript = heldCherry.GetComponent<Cherry>();
        if (cherryScript != null)
        {
            cherryScript.isHeld = true;
            cherryScript.playerHolding = gameObject;
        }

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

    public void CancelAimAndDrop()
    {
        if (heldCherry == null) return;

        if (pickupSource != null && cherryDropClip != null)
        {
            pickupSource.pitch = Random.Range(0.9f, 1.0f);
            pickupSource.PlayOneShot(cherryDropClip);
        }

        projectileScript?.CancelAim();

        Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
        heldCherry.transform.SetParent(null);
        if (rbCherry != null) rbCherry.isKinematic = false;

        SetCherryCollision(true);
        SetJiggle(true);

        if (animator != null) animator.SetBool("isPickingUp", false);

        Cherry cherryScript = heldCherry.GetComponent<Cherry>();
        if (cherryScript != null)
        {
            cherryScript.isHeld = false;
            cherryScript.playerHolding = null;
        }

        heldCherry = null;
        cherryPickupCooldown = 0.5f;
    }

    public void ForceDrop()
    {
        CancelAimAndDrop();
        heldCherry = null;
    }

    public void NotifyThrowEnded()
    {
        heldCherry = null;
        cherryPickupCooldown = 0.5f;
    }

    private IEnumerator PlayPickupAnimation()
    {
        animator.SetBool("isPickingUp", true);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
    }

    // ===== TRIGGER ZONES =====

    private void OnTriggerEnter(Collider other)
    {
        Playermovement pm = other.GetComponent<Playermovement>() ?? other.GetComponentInParent<Playermovement>();
        if (pm == null || pm.playerIndex == player.playerIndex) return;

        if (nearbyPlayer == null)
        {
            nearbyPlayer = pm.gameObject;
        }
        else
        {
            float distNew = Vector3.Distance(transform.position, pm.transform.position);
            float distCurrent = Vector3.Distance(transform.position, nearbyPlayer.transform.position);
            if (distNew < distCurrent) nearbyPlayer = pm.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Playermovement pm = other.GetComponent<Playermovement>() ?? other.GetComponentInParent<Playermovement>();
        if (pm != null && pm.gameObject == nearbyPlayer) nearbyPlayer = null;
    }

    // ===== HELPERS =====

    private void SetCherryCollision(bool enabled)
    {
        if (heldCherry == null) return;
        Collider[] playerCols = GetComponentsInChildren<Collider>();
        Collider[] cherryCols = heldCherry.GetComponentsInChildren<Collider>();
        foreach (var p in playerCols)
            foreach (var c in cherryCols)
                Physics.IgnoreCollision(p, c, !enabled);
    }

    private void SetJiggle(bool enabled)
    {
        if (jiggleParts == null) return;
        foreach (var jiggle in jiggleParts)
            if (jiggle != null) jiggle.enabled = enabled;
    }
}
