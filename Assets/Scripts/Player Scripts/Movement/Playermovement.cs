using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

// Merged from: Playermovement, PlayerDash
[RequireComponent(typeof(Rigidbody))]
public class Playermovement : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerID;
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

    [Header("Projectile Settings")]
    public Projectile projectileScript;

    [Header("Screen Shake")]
    public ScreenShake screenShake;

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

    // ===== DASH (merged from PlayerDash) =====

    [Header("Dash Settings")]
    public float dashForce = 15f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    public bool allowAirDash = true;
    [System.NonSerialized] public bool dashEnabled = true;

    [Header("Dash Combat")]
    public float dashHitForce = 10f;

    [Header("Dash UI")]
    public GameObject dashBarObject;
    public UnityEngine.UI.Image dashFillImage;

    [Header("Dash Audio")]
    [SerializeField] private AudioSource dashSource;
    [SerializeField] private AudioClip dashSound;

    [Header("Dash VFX")]
    [SerializeField] private ParticleSystem dashSmoke;

    private bool isDashing = false;
    private bool canDash = true;
    private float dashTimer;
    private float dashCooldownTimer = 0f;

    [Header("Event Effects")]
    public bool controlsReversed = false;

    // ===== SHARED =====

    [HideInInspector] public Gamepad assignedGamepad;
    private Rigidbody rb;
    public Animator animator;
    private Vector2 moveInput;
    public Vector3 initialSpawnPosition;
    private string currentSurface = "";

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        gc = GetComponent<GroundCheck>();

        if (projectileScript == null)
            projectileScript = GetComponent<Projectile>();

        if (projectileScript != null)
            projectileScript.SetOwner(this, playerID);

        if (screenShake == null)
            screenShake = FindFirstObjectByType<ScreenShake>();

        if (dashSource == null)
        {
            dashSource = gameObject.AddComponent<AudioSource>();
            dashSource.playOnAwake = false;
        }

        // Guard against prefab data missing new dash fields (serialize default = 0, not C# initializer)
        if (dashForce <= 0f) dashForce = 15f;
        if (dashDuration <= 0f) dashDuration = 0.3f;
        if (dashCooldown <= 0f) dashCooldown = 1f;
    }

    // ===== MOVEMENT =====

    private void FixedUpdate()
    {
        HandleDashPhysics();

        if (!canMove || isKnockedBack) return;

        /*moveInput = InputManager.Instance.GetMove(playerID);

        if (controlsReversed)
        {
            moveInput *= -1f;
        }*/

        Vector2 rawInput = InputManager.Instance.GetMove(playerID);

        moveInput = controlsReversed ? -rawInput : rawInput;

        HandleFootsteps();

        Transform cam = Camera.main.transform;
        Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = Vector3.Scale(cam.right, new Vector3(1, 0, 1)).normalized;
        Vector3 move = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        rb.linearVelocity = new Vector3(move.x * moveSpeed, rb.linearVelocity.y, move.z * moveSpeed);

        if (!gc.isGrounded)
        {
            if (rb.linearVelocity.y < 0)
                rb.AddForce(Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * rb.mass);
            else if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * rb.mass);
        }

        if (gc.isGrounded && isJumping)
            isJumping = false;

        if (animator != null)
        {
            float current = animator.GetFloat("Speed");
            float target = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
            float t = target < current ? Time.deltaTime * 20f : Time.deltaTime * 10f;
            if (!isJumping)
                animator.SetFloat("Speed", Mathf.Lerp(current, target, t));
            animator.SetBool("isGrounded", gc.isGrounded);
        }
    }

    private void Update()
    {
        if (allowJumpInput && gc.isGrounded && canMove && InputManager.Instance.GetJumpDown(playerID))
        {
            DoJump();
            animator.SetTrigger("Jump");
            isJumping = true;
        }

        HandleDashInput();
        HandleRotation();
    }

    private void HandleRotation()
    {
        //Vector2 input = InputManager.Instance.GetMove(playerID);
        Vector2 input = moveInput;
        isAiming = projectileScript != null && projectileScript.IsAiming();

        Transform cam = Camera.main.transform;
        Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = Vector3.Scale(cam.right, new Vector3(1, 0, 1)).normalized;

        if (input.sqrMagnitude > 0.01f)
        {
            Vector3 targetDir = (camForward * input.y + camRight * input.x).normalized;
            float speed = isAiming ? rotationSpeed * aimRotationMultiplier : rotationSpeed;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(targetDir, Vector3.up), Time.deltaTime * speed);
        }
    }

    public void DoJump()
    {
        if (!gc.isGrounded) return;
        if (jumpSource != null && jumpClip != null)
            jumpSource.PlayOneShot(jumpClip);
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }

    private void HandleFootsteps()
    {
        if (!gc.isGrounded || rb.linearVelocity.magnitude < 0.1f)
        {
            if (footstepSource.isPlaying) footstepSource.Stop();
            return;
        }

        if (Physics.Raycast(gc.origin, Vector3.down, out RaycastHit hit, 2f))
        {
            string newSurface = hit.collider.tag;
            if (newSurface != currentSurface)
            {
                if (newSurface == "Brick") footstepSource.clip = brickClip;
                else if (newSurface == "Grass") footstepSource.clip = grassClip;
                currentSurface = newSurface;
                if (footstepSource.isPlaying) footstepSource.Stop();
                footstepSource.Play();
            }
        }

        if (!footstepSource.isPlaying) footstepSource.Play();
    }

    public void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        if (rb == null) return;
        rb.linearVelocity = direction.normalized * force;
        isKnockedBack = true;
        StartCoroutine(EndKnockback(duration));
    }

    private IEnumerator EndKnockback(float time)
    {
        yield return new WaitForSeconds(time);
        isKnockedBack = false;
    }

    // ===== DASH =====

    private void HandleDashInput()
    {
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            dashCooldownTimer = Mathf.Max(0, dashCooldownTimer);
            if (dashFillImage != null)
                dashFillImage.fillAmount = dashCooldownTimer / dashCooldown;
        }

        PlayerEffects effects = GetComponent<PlayerEffects>();
        bool isBig = effects != null && effects.isBig;

        if (!dashEnabled || !canDash || !canMove || isBig) return;

        if (InputManager.Instance.GetDashDown(playerID))
        {
            if (gc.isGrounded || allowAirDash)
                StartDash();
        }
    }

    private void HandleDashPhysics()
    {
        if (!isDashing) return;

        dashTimer -= Time.fixedDeltaTime;
        if (dashTimer <= 0)
        {
            isDashing = false;
            canMove = true;
            if (dashSmoke != null) dashSmoke.Stop();
        }
    }

    private void StartDash()
    {
        isDashing = true;
        canDash = false;
        canMove = false;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        if (dashBarObject != null) dashBarObject.SetActive(true);

        if (dashSmoke != null) { dashSmoke.Clear(); dashSmoke.Play(); }
        if (dashSound != null && dashSource != null) dashSource.PlayOneShot(dashSound);

        Vector3 velocity = transform.forward * dashForce;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;

        Invoke(nameof(ResetDash), dashCooldown);
    }

    private void ResetDash()
    {
        canDash = true;
        if (dashBarObject != null) dashBarObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isDashing || !collision.gameObject.CompareTag("Player")) return;

        Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();
        if (otherRb == null) return;

        Playermovement otherPlayer = collision.gameObject.GetComponent<Playermovement>();
        Vector3 pushDir = (collision.transform.position - transform.position).normalized;
        pushDir.y = 0f;
        otherRb.linearVelocity = pushDir * dashHitForce;

        if (otherPlayer != null)
        {
            otherPlayer.isKnockedBack = true;
            StartCoroutine(EndDashKnockback(otherPlayer, 0.3f));
        }
    }

    private IEnumerator EndDashKnockback(Playermovement target, float time)
    {
        yield return new WaitForSeconds(time);
        target.isKnockedBack = false;
    }
}
