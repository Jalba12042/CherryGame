using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Playermovement : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerIndex = 0;
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Jump Tuning")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Throw Settings")]
    public Transform handHoldPoint;  // Where cherry sits
    public GameObject cherryPrefab;  // Prefab for throwing

    [Header("Projectile Settings")]
    public Projectile projectileScript;  // Link to projectile script

    [Header("Screen Shake")]
    public ScreenShake screenShake;  // Reference for screen shake effects


    [Header("State")]
    public bool canMove = true;
    public bool isGrounded;
    private bool jumpRequested = false;
    public bool isAiming = false;

    [HideInInspector] public Gamepad assignedGamepad;
    private Rigidbody rb;
    private Animator animator;
    private Vector2 moveInput;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // Try to find Projectile if not assigned
        if (projectileScript == null)
            projectileScript = GetComponent<Projectile>();

        if (projectileScript != null)
            projectileScript.SetOwner(this);

        // Try to find ScreenShake if not assigned
        if (screenShake == null)
            screenShake = FindFirstObjectByType<ScreenShake>();

        // Safety warning
        if (cherryPrefab == null)
            Debug.LogWarning($"{name} is missing a Cherry Prefab in the inspector!");

        if (handHoldPoint == null)
            Debug.LogWarning($"{name} is missing a Hand Hold Point in the inspector!");
    }


    private void FixedUpdate()
    {
        if (assignedGamepad == null || !canMove) return;

        // --- Movement ---
        moveInput = assignedGamepad.leftStick.ReadValue();
        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, groundCheckDistance, groundLayer);

        //Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        //Vector3 targetVelocity = move * moveSpeed;
        Transform cam = Camera.main.transform;

        // Flatten camera vectors (ignore Y)
        Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = Vector3.Scale(cam.right, new Vector3(1, 0, 1)).normalized;

        // Calculate camera-relative movement
        Vector3 move = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        Vector3 targetVelocity = move * moveSpeed;


        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        // --- Jump ---
        if (jumpRequested && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            jumpRequested = false;
        }

        // --- Enhanced Gravity ---
        if (!isGrounded)
        {
            if (rb.linearVelocity.y < 0)
                rb.AddForce(Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * rb.mass);
            else if (rb.linearVelocity.y > 0 && !assignedGamepad.buttonSouth.isPressed)
                rb.AddForce(Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * rb.mass);
        }

        // --- Animator Sync ---
        if (animator != null)
            animator.SetFloat("Speed", new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude);
            animator.SetBool("isGrounded", isGrounded);

    }

    private void Update()
    {
        if (assignedGamepad == null) return;

        // Jump input
        if (isGrounded && assignedGamepad.buttonSouth.wasPressedThisFrame && canMove)
            jumpRequested = true;

        // Rotation
        HandleRotation();
    }

    private void HandleRotation()
    {
        if (assignedGamepad == null) return;

        Vector2 moveInput = assignedGamepad.leftStick.ReadValue();
        Vector2 aimInput = assignedGamepad.rightStick.ReadValue();

        isAiming = projectileScript != null && projectileScript.IsAiming();

        Transform cam = Camera.main.transform;

        // Flattened camera vectors
        Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = Vector3.Scale(cam.right, new Vector3(1, 0, 1)).normalized;

        Vector3 targetDir = Vector3.zero;

        // --- PLAYER ROTATION ALWAYS FROM MOVEMENT ---
        if (moveInput.sqrMagnitude > 0.1f * 0.1f)
        {
            targetDir = (camForward * moveInput.y + camRight * moveInput.x).normalized;
        }

        // Apply rotation ONLY if move input exists
        if (targetDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(targetDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }
    }



    public Gamepad GetAssignedGamepad() => assignedGamepad;
}

