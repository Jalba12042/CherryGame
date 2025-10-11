using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerIndex = 0;
    public float moveForce = 30f;
    public float maxSpeed = 5f;
    public float turnSpeed = 3f;
    public float jumpForce = 7f;

    [Header("Active Ragdoll")]
    public Rigidbody hips;
    public Transform targetPose;
    public float uprightTorque = 500f;

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckDistance = 0.4f;
    public LayerMask groundLayer;

    [Header("Throw Settings")]
    public Transform handHoldPoint;
    public GameObject cherryPrefab;

    private Gamepad assignedGamepad;
    private Vector2 moveInput;
    private Vector2 smoothLookInput;
    private Vector3 lastLookDir = Vector3.forward;
    private bool jumpRequested = false;
    private bool isGrounded;

    private GameObject heldCherry;
    private bool isCharging;
    private GameObject nearbyCherry;
    public Projectile projectileScript;

    void Start()
    {
        if (Gamepad.all.Count > playerIndex)
            assignedGamepad = Gamepad.all[playerIndex];

        projectileScript = GetComponent<Projectile>();
        if (projectileScript != null)
            projectileScript.SetOwner(this);

        hips.interpolation = RigidbodyInterpolation.Interpolate;
        hips.maxAngularVelocity = 20f;

        hips.isKinematic = true;
        StartCoroutine(EnablePhysicsWhenGrounded());

        if (hips.mass < 5f)
            hips.mass = 10f;
    }

    IEnumerator EnablePhysicsWhenGrounded()
    {
        while (!Physics.Raycast(groundCheckPoint.position, Vector3.down, groundCheckDistance, groundLayer))
            yield return null;

        hips.isKinematic = false;
        Physics.SyncTransforms();
    }

    void Update()
    {
        if (assignedGamepad == null || hips == null) return;

        // --- Movement input ---
        moveInput = assignedGamepad.leftStick.ReadValue();

        // --- Jump input (only when grounded) ---
        if (assignedGamepad.buttonSouth.wasPressedThisFrame && isGrounded)
            jumpRequested = true;
        else if (!isGrounded)
            jumpRequested = false; // Prevent repeated jump while airborne

        // --- Look input ---
        Vector2 rawLook = assignedGamepad.rightStick.ReadValue();
        smoothLookInput = Vector2.Lerp(smoothLookInput, rawLook, Time.deltaTime * 15f);
        if (smoothLookInput.sqrMagnitude > 0.2f)
            lastLookDir = new Vector3(smoothLookInput.x, 0f, smoothLookInput.y).normalized;

        // --- Cherry pickup / drop ---
        float rtValue = assignedGamepad.rightTrigger.ReadValue();
        if (rtValue > 0.1f)
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
        else
        {
            if (heldCherry != null && !isCharging)
            {
                Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
                heldCherry.transform.SetParent(null);
                if (rbCherry != null)
                    rbCherry.isKinematic = false;

                heldCherry = null;
            }
        }
    }


    void FixedUpdate()
    {
        if (hips == null || targetPose == null) return;

        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, groundCheckDistance, groundLayer);

        // --- Keep upright (rotation only) ---
        Quaternion rotDiff = targetPose.rotation * Quaternion.Inverse(hips.rotation);
        rotDiff.ToAngleAxis(out float angle, out Vector3 axis);
        axis.Normalize();
        angle = Mathf.Clamp(angle, -45f, 45f);

        // Commented out torque
        // if (Mathf.Abs(angle) > 0.5f)
        //     hips.AddTorque(axis * uprightTorque * (angle / 45f) * Time.fixedDeltaTime, ForceMode.Acceleration);

        // --- Rotate toward look direction ---
        Vector3 forward = hips.transform.forward;
        float turnAngle = Vector3.SignedAngle(forward, lastLookDir, Vector3.up);

        // Commented out turn torque
        // hips.AddTorque(Vector3.up * turnAngle * turnSpeed, ForceMode.Acceleration);

        // --- Move character horizontally ---
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            Vector3 horizontalVel = new Vector3(hips.linearVelocity.x, 0f, hips.linearVelocity.z);

            // Commented out movement force
            // if (horizontalVel.magnitude < maxSpeed)
            //     hips.AddForce(moveDir * moveForce, ForceMode.Acceleration);
        }

        // --- Jump ---
        if (jumpRequested && isGrounded)
        {
            // Commented out jump force
            // hips.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequested = false;
        }

        // --- Parent follows hips ---
        transform.position = hips.position;
    }

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

    public Gamepad GetAssignedGamepad()
    {
        return assignedGamepad;
    }
}
