using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int playerIndex = 0;         // Your player index
    public Transform pickupTarget;      // Where the grabbed player should move to
    public float pickupRange = 2f;      // Range to detect other players
    private bool isCurrentlyGrabbed = false;


    private Gamepad assignedGamepad;
    private GameObject grabbedPlayerHip;
    private Rigidbody grabbedRigidbody;
    private PlayerPickup playerPickupScript; // Reference to own pickup script
    private PlayerEscapeUI grabbedEscapeUI; // Reference to escape UI on grabbed player

    [Header("Grab Cooldown")]
    public float grabCooldownTime = 1f;   // seconds before player can grab again
    private bool canGrab = true;          // whether the player is allowed to grab



    private void Start()
    {
        if (Gamepad.all.Count > playerIndex)
            assignedGamepad = Gamepad.all[playerIndex];
        playerPickupScript = GetComponent<PlayerPickup>();

    }

    void Update()
    {
        if (assignedGamepad == null) return;

        float rtValue = assignedGamepad.rightTrigger.ReadValue();

        if (rtValue > 0.1f)
        {
            if (grabbedPlayerHip == null && canGrab)
            {
                GameObject nearby = GetNearbyPlayer();
                if (nearby != null)
                {
                    grabbedPlayerHip = nearby;
                    grabbedRigidbody = grabbedPlayerHip.GetComponent<Rigidbody>();

                    if (grabbedRigidbody != null)
                        grabbedRigidbody.isKinematic = true;

                    // --- Assign grabber to the grabbed player ---
                    PlayerGrabbed grabbed = grabbedPlayerHip.GetComponent<PlayerGrabbed>();
                    if (grabbed != null)
                    {
                        grabbed.grabber = this; // 'this' is the PlayerPickup doing the grabbing
                    }

                    // --- Disable grabbed player's PlayerPickup ---
                    PlayerPickup grabbedPickup = grabbedPlayerHip.GetComponentInChildren<PlayerPickup>();
                    if (grabbedPickup != null)
                        grabbedPickup.StartBeingGrabbed();

                    // --- Show grabbed player's escape UI ---
                    PlayerEscapeUI escapeUI = grabbedPlayerHip.GetComponentInChildren<PlayerEscapeUI>();
                    PlayerController grabbedPC = grabbedPlayerHip.GetComponent<PlayerController>();
                    if (escapeUI != null && grabbedPC != null)
                    {
                        escapeUI.StartBeingGrabbed(grabbedPC.playerIndex);
                    }
                }
            }

        }
        else
        {
            if (grabbedPlayerHip != null)
            {
                if (grabbedRigidbody != null)
                    grabbedRigidbody.isKinematic = false;

                if (isCurrentlyGrabbed)
                {
                    PlayerPickup grabbedPickup = grabbedPlayerHip.GetComponentInChildren<PlayerPickup>();
                    if (grabbedPickup != null)
                        grabbedPickup.StopBeingGrabbed();

                    PlayerEscapeUI escapeUI = grabbedPlayerHip.GetComponentInChildren<PlayerEscapeUI>();
                    if (escapeUI != null)
                        escapeUI.StopBeingGrabbed();

                    isCurrentlyGrabbed = false;
                }

                grabbedPlayerHip = null;
                grabbedRigidbody = null;
            }
        }
    }


    private void LateUpdate()
    {
        // Move grabbed player to the pickup target
        if (grabbedPlayerHip != null && pickupTarget != null)
        {
            grabbedPlayerHip.transform.position = pickupTarget.position;
            grabbedPlayerHip.transform.rotation = pickupTarget.rotation;
        }
    }

    private GameObject GetNearbyPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange);

        foreach (var hit in hits)
        {
            // Ignore self
            PlayerController pc = hit.GetComponent<PlayerController>();
            if (pc == null || pc.playerIndex == playerIndex) continue;

            // Found a valid player to grab
            return hit.gameObject;
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }

    public void StartBeingGrabbed()
    {
        if (playerPickupScript != null)
            playerPickupScript.enabled = false;
    }

    public void StopBeingGrabbed()
    {
        if (playerPickupScript != null)
            playerPickupScript.enabled = true;
    }

    public void ReleaseCurrentGrabbedPlayer()
    {
        if (grabbedPlayerHip != null)
        {
            if (grabbedRigidbody != null)
                grabbedRigidbody.isKinematic = false;

            // Re-enable their PlayerPickup
            PlayerPickup grabbedPickup = grabbedPlayerHip.GetComponentInChildren<PlayerPickup>();
            if (grabbedPickup != null)
                grabbedPickup.StopBeingGrabbed();

            // Hide escape UI
            PlayerEscapeUI escapeUI = grabbedPlayerHip.GetComponentInChildren<PlayerEscapeUI>();
            if (escapeUI != null)
                escapeUI.StopBeingGrabbed();

            // DEBUG: Show which player is released
            PlayerController pc = grabbedPlayerHip.GetComponent<PlayerController>();
            if (pc != null)
                Debug.Log($"Player index {pc.playerIndex} released!");

            grabbedPlayerHip = null;
            grabbedRigidbody = null;

            // Optionally, start a cooldown here so the grabber can't immediately grab again
            StartCoroutine(GrabCooldown());
        }
    }

    public IEnumerator GrabCooldown()
    {
        canGrab = false;
        yield return new WaitForSeconds(grabCooldownTime);
        canGrab = true;
    }
}