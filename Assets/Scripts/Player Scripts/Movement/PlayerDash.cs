using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashForce = 15f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    public bool allowAirDash = true;

    [Header("Dash Combat")]
    public float hitForce = 10f;

    private Rigidbody rb;
    private Playermovement movement;
    private bool isDashing = false;
    private bool canDash = true;
    private float dashTimer;
    private GroundCheck gc;

    [Header("Dash UI")]
    public GameObject dashBarObject; // whole UI object
    public UnityEngine.UI.Image dashFillImage;
    private float dashCooldownTimer = 0f;

    [Header("Audio")]
    public AudioClip dashSound;
    private AudioSource audioSource; // NEW: Dedicated audio source

    [Header("Dash VFX")]
    [SerializeField] private ParticleSystem dashSmoke;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<Playermovement>();
        gc = GetComponent<GroundCheck>();

        // NEW: Foolproof AudioSource grabber
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            dashCooldownTimer = Mathf.Max(0, dashCooldownTimer);

            if (dashFillImage != null)
            {
                dashFillImage.fillAmount = dashCooldownTimer / dashCooldown;
            }
        }

        PlayerEffects effects = GetComponent<PlayerEffects>();
        bool isBig = effects != null && effects.isBig;

        if (!canDash || movement == null || !movement.canMove || isBig)
            return;

        bool dashPressed = false;

        if (!GameManager.Instance.isOnKeyboard)
        {
            if (movement.assignedGamepad != null &&
                movement.assignedGamepad.buttonWest.wasPressedThisFrame) // X on Xbox
            {
                dashPressed = true;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
                dashPressed = true;
        }

        if (dashPressed)
        {
            if (gc.isGrounded || allowAirDash)
            {
                StartDash();
            }
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;

            if (dashTimer <= 0)
            {
                isDashing = false;
                movement.canMove = true; // re-enable movement

                if (dashSmoke != null)
                {
                    dashSmoke.Stop(); // stops emitting but lets existing particles fade
                }
            }
        }
    }

    void StartDash()
    {
        isDashing = true;
        canDash = false;
        movement.canMove = false;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        if (dashBarObject != null)
            dashBarObject.SetActive(true);

        if (dashSmoke != null)
        {
            dashSmoke.Clear();   // wipes old particles
            dashSmoke.Play();    // start emitting
        }

        // NEW: Play the dash sound!
        if (dashSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dashSound);
        }

        Vector3 dashDirection = transform.forward;

        // Keep current vertical velocity (important for jumping)
        Vector3 newVelocity = dashDirection * dashForce;
        newVelocity.y = rb.linearVelocity.y;

        rb.linearVelocity = newVelocity;     

        Invoke(nameof(ResetDash), dashCooldown);
    }

    void ResetDash()
    {
        canDash = true;

        if (dashBarObject != null)
            dashBarObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only do this if YOU are currently dashing
        if (!isDashing) return;

        // Check if we hit another player
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();

            if (otherRb != null)
            {
                Playermovement otherPlayer = collision.gameObject.GetComponent<Playermovement>();

                Vector3 pushDir = (collision.transform.position - transform.position).normalized;
                pushDir.y = 0f;

                // Apply velocity directly (stronger than AddForce)
                otherRb.linearVelocity = pushDir * hitForce;

                if (otherPlayer != null)
                {
                    otherPlayer.isKnockedBack = true;
                    StartCoroutine(EndKnockback(otherPlayer, 0.3f));
                }
            }
        }
    }

    IEnumerator EndKnockback(Playermovement player, float time)
    {
        yield return new WaitForSeconds(time);
        player.isKnockedBack = false;
    }
}