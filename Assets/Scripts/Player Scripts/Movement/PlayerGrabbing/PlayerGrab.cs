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




    void Start()
    {
        player = GetComponent<Playermovement>();
        myCollider = GetComponent<Collider>();
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

        // Apply throw force
        thrownRb.isKinematic = false;
        thrownRb.linearVelocity = Vector3.zero;
        Vector3 throwDir = transform.forward;
        thrownRb.AddForce(throwDir * 10f + Vector3.up * 6f, ForceMode.Impulse);

        // Apply stun only to the thrown player
        if (thrownPm != null)
            StartCoroutine(thrownPm.GetComponent<PlayerGrab>().StunRoutine(2f));

        // Re-enable thrown player's grab component
        if (thrownGrab != null)
            thrownGrab.enabled = true;
    }



    public IEnumerator StunRoutine(float duration)
    {
        player.canMove = false;
        yield return new WaitForSeconds(duration);
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
