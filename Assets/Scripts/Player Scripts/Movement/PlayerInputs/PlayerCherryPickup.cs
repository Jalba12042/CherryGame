using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCherryPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int playerIndex = 0;
    public Transform handHoldPoint;   // Where the cherry should appear in hand
    public bool isCharging = false;   // Prevent drop while throwing
    public string heldLayerName = "HeldCherry"; // Layer to switch to while holding

    private Gamepad assignedGamepad;
    private GameObject heldCherry;
    private GameObject nearbyCherry;
    private Projectile projectileScript;
    private int originalLayer;

    void Start()
    {
        if (Gamepad.all.Count > playerIndex)
            assignedGamepad = Gamepad.all[playerIndex];

        // Get Projectile script from the parent PlayerController object
        projectileScript = GetComponentInParent<Projectile>();
    }

    void Update()
    {
        if (assignedGamepad == null)
            return;

        float rtValue = assignedGamepad.rightTrigger.ReadValue();

        // --- Pick up cherry ---
        if (rtValue > 0.1f)
        {
            if (heldCherry == null && nearbyCherry != null)
            {
                heldCherry = nearbyCherry;

                // Don't set kinematic here
                // Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
                // if (rbCherry != null)
                //     rbCherry.isKinematic = true;

                if (projectileScript != null)
                    projectileScript.PickUpCherry(heldCherry);
            }
        }
        // --- Drop cherry ---
        else
        {
            if (heldCherry != null && !isCharging)
            {
                // Don't set kinematic here either
                // Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
                // if (rbCherry != null)
                //     rbCherry.isKinematic = false;

                heldCherry = null;
            }
        }
    }

    void LateUpdate()
    {
        // Move the held cherry visually to the hand without affecting physics
        if (heldCherry != null)
        {
            heldCherry.transform.position = handHoldPoint.position;
            heldCherry.transform.rotation = handHoldPoint.rotation;
        }
    }


    // Trigger detection for nearby cherries
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
}
