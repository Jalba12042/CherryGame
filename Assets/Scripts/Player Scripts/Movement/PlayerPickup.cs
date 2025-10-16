using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int playerIndex = 0;         // Your player index
    public Transform pickupTarget;      // Where the grabbed player should move to
    public float pickupRange = 2f;      // Range to detect other players

    private Gamepad assignedGamepad;
    private GameObject grabbedPlayerHip;
    private Rigidbody grabbedRigidbody;
    private PlayerPickup playerPickupScript; // Reference to own pickup script
    private PlayerEscapeUI grabbedEscapeUI; // Reference to escape UI on grabbed player



    private void Start()
    {
        if (Gamepad.all.Count > playerIndex)
            assignedGamepad = Gamepad.all[playerIndex];
        playerPickupScript = GetComponent<PlayerPickup>();

    }

    private void Update()
    {
        if (assignedGamepad == null) return;

        float rtValue = assignedGamepad.rightTrigger.ReadValue();

        if (rtValue > 0.1f)
        {
            // Attempt to grab another player if nothing is grabbed
            if (grabbedPlayerHip == null)
            {
                GameObject nearby = GetNearbyPlayer();
                if (nearby != null)
                {
                    grabbedPlayerHip = nearby;
                    grabbedRigidbody = grabbedPlayerHip.GetComponent<Rigidbody>();

                    if (grabbedRigidbody != null)
                        grabbedRigidbody.isKinematic = true; // freeze physics while carrying

                    // --- DISABLE the grabbed player's PlayerPickup ---
                    PlayerPickup grabbedPickup = grabbedPlayerHip.GetComponentInChildren<PlayerPickup>();
                    if (grabbedPickup != null)
                        grabbedPickup.StartBeingGrabbed();

                    // --- SHOW the grabbed player's escape UI ---
                    PlayerEscapeUI escapeUI = grabbedPlayerHip.GetComponentInChildren<PlayerEscapeUI>();
                    if (escapeUI != null)
                        escapeUI.StartBeingGrabbed();
                }
            }
        }
        else
        {
            // Release grabbed player
            if (grabbedPlayerHip != null)
            {
                if (grabbedRigidbody != null)
                    grabbedRigidbody.isKinematic = false;

                // --- RE-ENABLE the grabbed player's PlayerPickup ---
                PlayerPickup grabbedPickup = grabbedPlayerHip.GetComponentInChildren<PlayerPickup>();
                if (grabbedPickup != null)
                    grabbedPickup.StopBeingGrabbed();

                // --- HIDE the grabbed player's escape UI ---
                PlayerEscapeUI escapeUI = grabbedPlayerHip.GetComponentInChildren<PlayerEscapeUI>();
                if (escapeUI != null)
                    escapeUI.StopBeingGrabbed();

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
            // Basically do the same as RT release
            if (grabbedRigidbody != null)
                grabbedRigidbody.isKinematic = false;

            // Re-enable their pickup
            PlayerPickup grabbedPickup = grabbedPlayerHip.GetComponentInChildren<PlayerPickup>();
            if (grabbedPickup != null)
                grabbedPickup.StopBeingGrabbed();

            // Hide escape UI
            if (grabbedEscapeUI != null)
            {
                grabbedEscapeUI.StopBeingGrabbed();
                grabbedEscapeUI = null;
            }

            grabbedPlayerHip = null;
            grabbedRigidbody = null;
        }
    }


}
