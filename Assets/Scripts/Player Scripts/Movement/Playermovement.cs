using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerIndex = 0; // Set this when the player is spawned
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Throw Settings")]
    public Transform handHoldPoint;        // Where cherry sits
    public GameObject cherryPrefab;        // Prefab for throwing
    public float minThrowForce = 5f;
    public float maxThrowForce = 15f;
    public LineRenderer lineRenderer;      // Assign in inspector
    public int arcResolution = 30;
    public float timeStep = 0.1f;

    private Gamepad assignedGamepad;
    private Rigidbody rb;
    private bool isGrounded = true;

    private GameObject heldCherry;
    private bool isCharging;
    private float currentThrowForce;

    private GameObject nearbyCherry;

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

        if (lineRenderer != null)
            lineRenderer.positionCount = 0; // clear trajectory at start
    }

    void Update()
    {
        if (assignedGamepad == null || rb == null) return;

        // --- Movement (Left Stick) ---
        Vector2 moveInput = assignedGamepad.leftStick.ReadValue();
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);

        // --- Rotation (Right Stick) ---
        Vector2 lookInput = assignedGamepad.rightStick.ReadValue();
        Vector3 lookDir = new Vector3(lookInput.x, 0f, lookInput.y);
        if (lookDir.sqrMagnitude > 0.1f) // only rotate if stick is moved
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 15f // rotation speed
            );
        }

        // --- Jump (Button South) ---
        if (isGrounded && assignedGamepad.buttonSouth.wasPressedThisFrame)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        // --- Pickup / Drop (RT = rightTrigger) ---
        float rtValue = assignedGamepad.rightTrigger.ReadValue();
        if (rtValue > 0.1f) // holding RT
        {
            if (heldCherry == null && nearbyCherry != null)
            {
                // Pick up the cherry
                heldCherry = nearbyCherry;
                Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
                if (rbCherry != null)
                    rbCherry.isKinematic = true;

                heldCherry.transform.SetParent(handHoldPoint);
                heldCherry.transform.localPosition = Vector3.zero;
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

        // --- Throw (LT = leftTrigger) ---
        if (heldCherry != null)
        {
            float ltValue = assignedGamepad.leftTrigger.ReadValue();

            if (ltValue > 0.1f) // holding trigger
            {
                isCharging = true;
                currentThrowForce = Mathf.Lerp(minThrowForce, maxThrowForce, ltValue);

                ShowTrajectory();
            }
            else if (isCharging) // released trigger
            {
                ThrowCherry();
                isCharging = false;

                if (lineRenderer != null)
                    lineRenderer.positionCount = 0; // clear trajectory
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

    // Throw cherry
    private void ThrowCherry()
    {
        if (heldCherry == null) return;

        Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
        heldCherry.transform.SetParent(null);
        rbCherry.isKinematic = false;

        // Use last aim direction
        rbCherry.linearVelocity = GetAimVelocity();
        //rbCherry.linearVelocity = throwDir * currentThrowForce;

        heldCherry = null;
    }

    // Trajectory arc
    private void ShowTrajectory()
    {
        if (lineRenderer == null) return;

        Vector3 startPos = handHoldPoint.position;
        Vector3 startVel = GetAimVelocity();

        lineRenderer.positionCount = arcResolution;

        float minY = 0.2f; // minimum height for the line (slightly above ground)

        for (int i = 0; i < arcResolution; i++)
        {
            float t = i * timeStep;
            Vector3 pos = startPos + startVel * t + 0.5f * Physics.gravity * t * t;

            // Clamp Y so it doesn't go under the ground
            if (pos.y < minY)
                pos.y = minY;

            lineRenderer.SetPosition(i, pos);
        }
    }


    // Helper: Get aim direction (right stick or forward if neutral)
    private Vector3 GetAimVelocity()
    {
        Vector2 lookInput = assignedGamepad.rightStick.ReadValue();
        Vector3 lookDir = new Vector3(lookInput.x, 0f, lookInput.y);

        if (lookDir.sqrMagnitude < 0.1f)
            lookDir = transform.forward;

        lookDir.Normalize();

        // --- Separate horizontal and vertical speeds ---
        float horizontalSpeed = 5f; // <<< smaller value shortens distance
        float verticalSpeed = 7f;   // <<< controls height of the arc

        Vector3 velocity = lookDir * horizontalSpeed + Vector3.up * verticalSpeed;
        return velocity;
    }
}
