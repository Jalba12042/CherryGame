using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Playermovement : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerIndex = 0; // Set this when the player is spawned
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Throw Settings")]
    public Transform handHoldPoint; // Where cherry sits
    public GameObject cherryPrefab; // Prefab for throwing

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Pickup Settings")]
    public Transform pickupTarget;      // Where the grabbed player should move to
    public float pickupRange = 2f;      // Range to detect other players
    public float grabCooldownTime = 1f; // seconds before player can grab again


    public Gamepad assignedGamepad;
    private Rigidbody rb;
    private bool isGrounded;
    private bool jumpRequested = false;
    private Vector2 moveInput;

    private GameObject heldCherry;
    private bool isCharging;
    private GameObject nearbyCherry;

    private bool canGrab = true;
    private GameObject grabbedPlayer;
    private Rigidbody grabbedRigidbody;
    private bool isCurrentlyGrabbed = false;

    public Projectile projectileScript;
    private Vector2 smoothLookInput;
    private Vector3 lastLookDir = Vector3.forward;

    // --- Sprinkler Interaction Fields ---
    [Header("Sprinkler Interaction")]
    [Tooltip("How strongly the sprinkler pushes the player back.")]
    public float pushbackResistance = 1f; // higher = less push
    [Tooltip("If true, player is currently slowed by water.")]
    public bool IsSlowed = false;
    private float originalMoveSpeed;

    void Start()
    {
        if (Gamepad.all.Count > playerIndex)
            assignedGamepad = Gamepad.all[playerIndex];

        rb = GetComponent<Rigidbody>();
        projectileScript = GetComponent<Projectile>();

        originalMoveSpeed = moveSpeed;
    }

    private void FixedUpdate()
    {
        // --- Movement (Left Stick) ---
        moveInput = assignedGamepad != null ? assignedGamepad.leftStick.ReadValue() : Vector2.zero;
        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, groundCheckDistance, groundLayer);

        if (isGrounded && moveInput == Vector2.zero)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        Vector3 targetVelocity = move * moveSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        // --- Jump (Button South) ---
        if (jumpRequested)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequested = false;
        }
    }

    void Update()
    {
        if (assignedGamepad == null || rb == null)
            return;

        // --- Jump Input ---
        if (isGrounded && assignedGamepad.buttonSouth.wasPressedThisFrame)
        {
            jumpRequested = true;
        }

        // --- Rotation (Right Stick) ---
        Vector2 rawLook = assignedGamepad.rightStick.ReadValue();
        smoothLookInput = Vector2.Lerp(smoothLookInput, rawLook, Time.deltaTime * 15f);
        if (smoothLookInput.sqrMagnitude > 0.2f)
        {
            lastLookDir = new Vector3(smoothLookInput.x, 0f, smoothLookInput.y).normalized;
        }
        Quaternion targetRotation = Quaternion.LookRotation(lastLookDir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 12f);

        // --- Pickup / Drop Cherry (RT) ---
        float rtValue = assignedGamepad.rightTrigger.ReadValue();
        if (rtValue > 0.1f)
        {
            HandleCherryPickup();
            HandlePlayerGrab();
        }
        else
        {
            HandleCherryDrop();
            HandlePlayerRelease();
        }
    }

    // --- Handle Player Pickup Logic ---
    private void HandlePlayerGrab()
    {
        if (grabbedPlayer == null && canGrab)
        {
            GameObject nearby = GetNearbyPlayer();
            if (nearby != null)
            {
                grabbedPlayer = nearby;
                grabbedRigidbody = grabbedPlayer.GetComponent<Rigidbody>();

                if (grabbedRigidbody != null)
                    grabbedRigidbody.isKinematic = true;

                // Assign grabber reference
                PlayerGrabbed grabbed = grabbedPlayer.GetComponent<PlayerGrabbed>();
                if (grabbed != null)
                    grabbed.grabber = this;

                // Temporarily disable movement on grabbed player
                Playermovement grabbedMove = grabbedPlayer.GetComponent<Playermovement>();
                if (grabbedMove != null)
                    grabbedMove.enabled = false;

                // Trigger escape UI
                PlayerEscapeUI escapeUI = grabbedPlayer.GetComponentInChildren<PlayerEscapeUI>();
                if (escapeUI != null)
                    escapeUI.StartBeingGrabbed(grabbedMove.playerIndex);

                isCurrentlyGrabbed = true;
            }
        }

        if (grabbedPlayer != null && pickupTarget != null)
        {
            grabbedPlayer.transform.position = pickupTarget.position;
            grabbedPlayer.transform.rotation = pickupTarget.rotation;
        }
    }

    public void HandlePlayerRelease()
    {
        if (grabbedPlayer != null)
        {
            if (grabbedRigidbody != null)
                grabbedRigidbody.isKinematic = false;

            if (isCurrentlyGrabbed)
            {
                Playermovement grabbedMove = grabbedPlayer.GetComponent<Playermovement>();
                if (grabbedMove != null)
                    grabbedMove.enabled = true;

                PlayerEscapeUI escapeUI = grabbedPlayer.GetComponentInChildren<PlayerEscapeUI>();
                if (escapeUI != null)
                    escapeUI.StopBeingGrabbed();

                isCurrentlyGrabbed = false;
            }

            grabbedPlayer = null;
            grabbedRigidbody = null;

            StartCoroutine(GrabCooldown());
        }
    }

    private GameObject GetNearbyPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange);
        foreach (var hit in hits)
        {
            Playermovement pc = hit.GetComponent<Playermovement>();
            if (pc == null || pc.playerIndex == playerIndex) continue;
            return hit.gameObject;
        }
        return null;
    }


    public IEnumerator GrabCooldown()
    {
        canGrab = false;
        yield return new WaitForSeconds(grabCooldownTime);
        canGrab = true;
    }

    // --- Handle Cherry Logic ---
    private void HandleCherryPickup()
    {
        if (heldCherry == null && nearbyCherry != null)
        {
            heldCherry = nearbyCherry;
            Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
            if (rbCherry != null) rbCherry.isKinematic = true;
            heldCherry.transform.SetParent(handHoldPoint);
            heldCherry.transform.localPosition = Vector3.zero;
            if (projectileScript != null) projectileScript.PickUpCherry(heldCherry);
        }
    }

    private void HandleCherryDrop()
    {
        if (heldCherry != null && !isCharging)
        {
            Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
            heldCherry.transform.SetParent(null);
            if (rbCherry != null) rbCherry.isKinematic = false;
            heldCherry = null;
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

    public Gamepad GetAssignedGamepad() => assignedGamepad;

    // --- Sprinkler Interaction API (called from SprinklerInteraction.cs) ---

    public void ApplyPushback(Vector3 direction, float force)
    {
        if (rb == null) return;
        Vector3 push = direction.normalized * (force / pushbackResistance);
        rb.AddForce(push, ForceMode.VelocityChange);
    }

    public void ApplySlow(float slowMultiplier, float duration)
    {
        if (IsSlowed) return; // prevent stacking

        StartCoroutine(SlowRoutine(slowMultiplier, duration));
    }

    private IEnumerator SlowRoutine(float slowMultiplier, float duration)
    {
        IsSlowed = true;
        moveSpeed *= slowMultiplier;
        yield return new WaitForSeconds(duration);
        moveSpeed = originalMoveSpeed;
        IsSlowed = false;
    }
}
