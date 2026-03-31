using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashForce = 15f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    public bool allowAirDash = true;

    private Rigidbody rb;
    private Playermovement movement;
    private bool isDashing = false;
    private bool canDash = true;

    private float dashTimer;

    [Header("Dash UI")]
    public GameObject dashBarObject; // whole UI object
    public UnityEngine.UI.Image dashFillImage;

    private float dashCooldownTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<Playermovement>();
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

        if (!canDash || movement == null || !movement.canMove)
            return;

        bool dashPressed = false;

        if (!GameManager.Instance.isOnKeyboard)
        {
            if (movement.assignedGamepad != null &&
                movement.assignedGamepad.buttonWest.wasPressedThisFrame) // X on Xbox
            {
                Debug.Log("Dash Button Pressed");
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
            if (movement.isGrounded || allowAirDash)
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
}