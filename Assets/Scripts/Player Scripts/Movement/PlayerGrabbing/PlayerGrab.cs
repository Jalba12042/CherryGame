using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerGrab : MonoBehaviour
{
    public Transform pickupTarget;
    public float pickupRange = 2f;
    public float grabCooldownTime = 1f;

    public Playermovement player;
    private Gamepad gamepad;
    private GameObject grabbedPlayer;
    private Rigidbody grabbedRigidbody;
    private Collider myCollider, grabbedCollider;
    private bool canGrab = true;

    private GameObject nearbyPlayer;
    [HideInInspector] public bool isGrabbed = false; // new flag for grabbed state

    private bool grabEscapeCooldown = false; // prevents grab right after escape
    public float escapeCooldownTime = 1f;    // duration of cooldown after escape
    private Animator animator;
    [SerializeField] private Animator faceAnimator;
    public GameObject stunCanvas;
    public TMPro.TMP_Text stunTimerText;



    private void Awake()
    {
        if (faceAnimator == null)
            faceAnimator = GetComponentInChildren<Animator>();
    }


    void Start()
    {
        player = GetComponent<Playermovement>();
        myCollider = GetComponent<Collider>();
        animator = GetComponent<Animator>();

        if (stunCanvas != null)
            stunCanvas.SetActive(false);

    }

    void Update()
    {
        if (player.assignedGamepad == null) return;
        gamepad = player.assignedGamepad;

        // Keep grabbed player at pickup target
        if (grabbedPlayer != null && pickupTarget != null)
        {
            grabbedPlayer.transform.position = pickupTarget.position;
            grabbedPlayer.transform.rotation = pickupTarget.rotation;
        }

        if (grabEscapeCooldown) return;

        if (gamepad.rightTrigger.wasPressedThisFrame)
            TryGrab();
        else if (gamepad.rightTrigger.wasReleasedThisFrame)
            ReleaseGrab();
        else if (gamepad.leftTrigger.wasReleasedThisFrame && grabbedPlayer != null)
            ThrowGrabbedPlayer();
    }


    private void TryGrab()
    {
        if (!canGrab || grabbedPlayer != null || nearbyPlayer == null || isGrabbed) return;

        PlayerEffects pe = nearbyPlayer.GetComponent<PlayerEffects>();
        if (pe != null && pe.isBig)
            return;

        grabbedPlayer = nearbyPlayer;
        grabbedRigidbody = grabbedPlayer.GetComponent<Rigidbody>();
        if (grabbedRigidbody != null)
            grabbedRigidbody.isKinematic = true;

        grabbedCollider = grabbedPlayer.GetComponent<Collider>();
        if (grabbedCollider != null)
            Physics.IgnoreCollision(myCollider, grabbedCollider, true);

        Playermovement pm = grabbedPlayer.GetComponent<Playermovement>();
        if (pm != null)
            pm.canMove = false;

        PlayerGrab grabbedGrab = grabbedPlayer.GetComponent<PlayerGrab>();
        if (grabbedGrab != null)
            grabbedGrab.isGrabbed = true;

        PlayerEscapeUI escapeUI = grabbedPlayer.GetComponent<PlayerEscapeUI>();
        if (escapeUI != null)
        {
            escapeUI.StartBeingGrabbed(this);
        }

        if (animator != null)
            animator.SetBool("isPickingUp", true);

    }



    public void ReleaseGrab()
    {
        if (grabbedPlayer == null) return;

        if (grabbedRigidbody != null)
            grabbedRigidbody.isKinematic = false;

        if (grabbedCollider != null)
            Physics.IgnoreCollision(myCollider, grabbedCollider, false);

        if (grabbedPlayer != null)
        {
            Playermovement pm = grabbedPlayer.GetComponent<Playermovement>();
            if (pm != null)
                pm.canMove = true; // restore movement

            // Stop Escape UI for grabbed player
            var escapeUI = grabbedPlayer.GetComponent<PlayerEscapeUI>();
            if (escapeUI != null)
                escapeUI.StopBeingGrabbed();

            // Reset grabbed player's grab flag
            PlayerGrab grabbedGrab = grabbedPlayer.GetComponent<PlayerGrab>();
            if (grabbedGrab != null)
            {
                grabbedGrab.isGrabbed = false;
                grabbedGrab.enabled = true;
            }

            if (animator != null)
                animator.SetBool("isPickingUp", false);

        }

        grabbedPlayer = null;
        grabbedRigidbody = null;
        grabbedCollider = null;

        StartCoroutine(GrabCooldown());
    }

    private void ThrowGrabbedPlayer()
    {
        if (grabbedPlayer == null || grabbedRigidbody == null) return;

        // Store references locally before releasing
        GameObject thrownPlayer = grabbedPlayer;
        Rigidbody thrownRb = grabbedRigidbody;
        PlayerGrab thrownGrab = grabbedPlayer.GetComponent<PlayerGrab>();
        Playermovement thrownPm = grabbedPlayer.GetComponent<Playermovement>();

        // Release the grab after storing references
        ReleaseGrab();

        if (animator != null)
            animator.SetBool("isPickingUp", false);

        // Apply throw force
        thrownRb.isKinematic = false;
        thrownRb.linearVelocity = Vector3.zero;
        Vector3 throwDir = transform.forward;
        thrownRb.AddForce(throwDir * 10f + Vector3.up * 6f, ForceMode.Impulse);

        // Apply stun only to the thrown player
        if (thrownPm != null)
            StartCoroutine(thrownPm.GetComponent<PlayerGrab>().StunRoutine(5f));

        // Re-enable thrown player's grab component
        if (thrownGrab != null)
            thrownGrab.enabled = true;
    }


    public IEnumerator StunRoutine(float duration)
    {
        player.canMove = false;

        // Freeze random blinking
        RandomFaceChanger rfc = GetComponentInChildren<RandomFaceChanger>();
        if (rfc != null)
            rfc.PauseFaces();

        if (faceAnimator != null)
            faceAnimator.SetBool("isStunned", true);

        // ----- ENABLE UI -----
        if (stunCanvas != null)
            stunCanvas.SetActive(true);

        float remaining = duration;

        while (remaining > 0f)
        {
            if (stunTimerText != null)
                stunTimerText.text = Mathf.Ceil(remaining).ToString(); // 5,4,3,2,1

            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }

        // Turn off stunned animation
        if (faceAnimator != null)
            faceAnimator.SetBool("isStunned", false);

        // Resume blinking
        if (rfc != null)
            rfc.ResumeFaces();

        // ----- DISABLE UI -----
        if (stunCanvas != null)
            stunCanvas.SetActive(false);

        player.canMove = true;
    }





    private void OnTriggerEnter(Collider other)
    {
        Playermovement pm = other.GetComponent<Playermovement>();
        if (pm != null && pm.playerIndex != player.playerIndex)
        {
            nearbyPlayer = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == nearbyPlayer)
            nearbyPlayer = null;
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

    private IEnumerator EscapeGrabCooldown()
    {
        grabEscapeCooldown = true;
        yield return new WaitForSeconds(escapeCooldownTime);
        grabEscapeCooldown = false;
    }
}
