using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerIndex = 0; // Set this when the player is spawned
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Throw Settings")]
    public Transform handHoldPoint;        // Where cherry sits
    public GameObject cherryPrefab;        // Prefab for throwing

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Gamepad assignedGamepad;
    private Rigidbody rb;
    private bool isGrounded;
    private bool jumpRequested = false;

    private GameObject heldCherry;
    private bool isCharging;


    private GameObject nearbyCherry;

    public Projectile projectileScript;

    void Start()
    {
        // Assign controller
        if (Gamepad.all.Count > playerIndex)
        {
            assignedGamepad = Gamepad.all[playerIndex];
        }
        else
        {
            Debug.LogWarning($"No gamepad found for player {playerIndex}");
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("PlayerMovement requires a Rigidbody component.");
        }
    }

    private void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);

        // --- Jump (Button South) ---
        if (jumpRequested)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequested = false;
        }

        // --- Movement (Left Stick) ---
        Vector2 moveInput = assignedGamepad.leftStick.ReadValue();
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        Vector3 targetVelocity = move * moveSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

    }
    void Update()
    {
        if (assignedGamepad == null || rb == null) return;

        if (isGrounded && assignedGamepad.buttonSouth.wasPressedThisFrame)
        {
            jumpRequested = true;
        }

            // --- Rotation (Right Stick) ---
            Vector2 lookInput = assignedGamepad.rightStick.ReadValue();
        Vector3 lookDir = new Vector3(lookInput.x, 0f, lookInput.y);
        if (lookDir.sqrMagnitude > 0.1f) // only rotate if stick is moved
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 5f // rotation speed
            );
        }

        // --- Pickup / Drop (RT = rightTrigger) ---
        float rtValue = assignedGamepad.rightTrigger.ReadValue();
        if (rtValue > 0.1f) // holding RT
        {
            if (heldCherry == null && nearbyCherry != null)
            {
                heldCherry = nearbyCherry;
                Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
                if (rbCherry != null)
                    rbCherry.isKinematic = true;

                heldCherry.transform.SetParent(handHoldPoint);
                heldCherry.transform.localPosition = Vector3.zero;

                if (projectileScript != null)
                    projectileScript.PickUpCherry(heldCherry);
            }
        }

        else // released RT
        {
            if (heldCherry != null && !isCharging) // don't drop while throwing
            {
                Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
                heldCherry.transform.SetParent(null);
                if (rbCherry != null)
                    rbCherry.isKinematic = false;

                heldCherry = null;
            }
        }

    }

    // Pick up cherry
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cherry"))
        {
            nearbyCherry = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Cherry") && other.gameObject == nearbyCherry)
        {
            nearbyCherry = null;
        }
    }
}
