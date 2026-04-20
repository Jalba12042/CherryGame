using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Playermovement : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerIndex = 0;
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public bool isSlowed = false;

    [Header("Jump Tuning")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public bool allowJumpInput = true;
    private bool isJumping = false;


    [Header("Ground Check")]
    public GroundCheck gc;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 10f;

    [Range(0.05f, 1f)]
    public float aimRotationMultiplier = 0.35f;

    [Header("Throw Settings")]
    public Transform handHoldPoint;  // Where cherry sits
    public GameObject cherryPrefab;  // Prefab for throwing

    [Header("Projectile Settings")]
    public Projectile projectileScript;  // Link to projectile script

    [Header("Screen Shake")]
    public ScreenShake screenShake;  // Reference for screen shake effects


    [Header("State")]
    public bool canMove = true;
    public bool isAiming = false;

    [Header("Knockback")]
    public bool isKnockedBack = false;

    [Header("Footstep Audio")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip grassClip;
    [SerializeField] private AudioClip brickClip;

    [Header("Jump Audio")]
    [SerializeField] private AudioSource jumpSource;
    [SerializeField] private AudioClip jumpClip;

    private string currentSurface = "";

    [HideInInspector] public Gamepad assignedGamepad;
    private Rigidbody rb;
    public Animator animator;
    private Vector2 moveInput;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        gc = GetComponent<GroundCheck>();

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
        //if (assignedGamepad == null || !canMove) return;
        if (!canMove || isKnockedBack) return;

        // --- Movement ---
        moveInput = GameManager.Instance.isOnKeyboard ? new Vector2(Input.GetAxis("HorizontalWASD"), Input.GetAxis("VerticalWASD")) : assignedGamepad.leftStick.ReadValue();

        HandleFootsteps();

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


        // --- Enhanced Gravity (fixed jump height) ---
        if (!gc.isGrounded)
        {
            if (rb.linearVelocity.y < 0)
            {
                // Falling — stronger gravity
                rb.AddForce(Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * rb.mass);
            }
            else if (rb.linearVelocity.y > 0)
            {
                // Rising — always apply same extra gravity
                rb.AddForce(Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * rb.mass);
            }
        }

        if (gc.isGrounded && isJumping)
        {
            isJumping = false;
        }


        if (animator != null)
        {
            float currentSpeed = animator.GetFloat("Speed");
            float targetSpeed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;

            float smoothSpeed;
            if (targetSpeed < currentSpeed)
            {
                // Decelerating → snap faster
                smoothSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 20f);
            }
            else
            {
                // Accelerating → smoother
                smoothSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 10f);
            }

            if (!isJumping)
            {
                animator.SetFloat("Speed", smoothSpeed);
            }
            animator.SetBool("isGrounded", gc.isGrounded);

        }


    }

    private void Update()
    {
        //if (assignedGamepad == null) return;

        // Jump input
        if (!GameManager.Instance.isOnKeyboard)
        {
            if (allowJumpInput && gc.isGrounded && assignedGamepad.buttonSouth.wasPressedThisFrame && canMove)
            {
                DoJump();
                animator.SetTrigger("Jump"); // fire animation immediately
                isJumping = true;
            }
        }
        else
        {
            if (allowJumpInput && gc.isGrounded && Input.GetKeyDown(KeyCode.Space) && canMove)
            {
                DoJump();
                animator.SetTrigger("Jump");
                isJumping = true;
            }
        }


        // Rotation
        HandleRotation();
    }

    private void HandleRotation()
    {
        //if (assignedGamepad == null) return;

        Vector2 moveInput = GameManager.Instance.isOnKeyboard ? new Vector2(Input.GetAxis("HorizontalWASD"), Input.GetAxis("VerticalWASD")) : assignedGamepad.leftStick.ReadValue();
        Vector2 aimInput = GameManager.Instance.isOnKeyboard ? new Vector2(Input.GetAxis("HorizontalArrows"), Input.GetAxis("VerticalArrows")) : assignedGamepad.rightStick.ReadValue();
        //Vector2 moveInput = assignedGamepad.leftStick.ReadValue();
        //Vector2 aimInput = assignedGamepad.rightStick.ReadValue();

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

            float appliedRotationSpeed = rotationSpeed;

            // Slow rotation when aiming  
            if (isAiming)
                appliedRotationSpeed *= aimRotationMultiplier;

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * appliedRotationSpeed);

        }
    }


    public void DoJump()
    {
        // Only jump if still grounded at the moment of the event
        if (!gc.isGrounded) return;

        // Play jump sound
        if (jumpSource != null && jumpClip != null)
        {
            jumpSource.PlayOneShot(jumpClip);
        }

        // Reset vertical velocity for consistent jumps
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Apply the actual jump force
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }

    private void HandleFootsteps()
    {
        if (!gc.isGrounded || rb.linearVelocity.magnitude < 0.1f)
        {
            if (footstepSource.isPlaying)
                footstepSource.Stop();
            return;
        }

        if (Physics.Raycast(gc.origin, Vector3.down, out RaycastHit hit, 2f))
        {
            string newSurface = hit.collider.tag;

            // Only change if surface changed
            if (newSurface != currentSurface)
            {
                if (newSurface == "Brick")
                {
                    footstepSource.clip = brickClip;
                    Debug.Log("Switched from " + currentSurface + " to Brick");
                }
                else if (newSurface == "Grass")
                {
                    footstepSource.clip = grassClip;
                    Debug.Log("Switched from " + currentSurface + " to Grass");
                }

                currentSurface = newSurface;

                if (footstepSource.isPlaying)
                    footstepSource.Stop();

                footstepSource.Play();
            }
        }

        if (!footstepSource.isPlaying)
        {
            footstepSource.Play();
        }
    }


    /*private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Brick"))
        {
            currentSurface = "Brick";
            footstepSource.clip = brickClip;
            Debug.Log("Entered Brick");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Brick"))
        {
            currentSurface = "Grass";
            footstepSource.clip = grassClip;
            Debug.Log("Exited Brick");
        }
    }

    private void HandleFootsteps()
    {
        if (!isGrounded || rb.linearVelocity.magnitude < 0.1f)
        {
            if (footstepSource.isPlaying)
                footstepSource.Stop();
            return;
        }

        if (!footstepSource.isPlaying)
        {
            footstepSource.Play();
        }
    }*/

    public Gamepad GetAssignedGamepad() => assignedGamepad;
}

