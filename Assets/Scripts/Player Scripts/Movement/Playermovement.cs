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

        // Movement (Left Stick)
        Vector2 moveInput = assignedGamepad.leftStick.ReadValue();
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);

        // Rotation (Right Stick)
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

        // Jump
        if (isGrounded && assignedGamepad.buttonSouth.wasPressedThisFrame)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        // Throw input (LT = leftTrigger, 0–1)
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
                    lineRenderer.positionCount = 0; // clear arc
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    // Pick up cherry
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cherry") && heldCherry == null)
        {
            heldCherry = other.gameObject;
            Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
            if (rbCherry != null)
            {
                rbCherry.isKinematic = true; // Stops physics while held
            }
            heldCherry.transform.SetParent(handHoldPoint);
            heldCherry.transform.localPosition = Vector3.zero;
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
        Vector3 throwDir = GetAimDirection();
        rbCherry.linearVelocity = throwDir * currentThrowForce;

        heldCherry = null;
    }

    // Trajectory arc
    private void ShowTrajectory()
    {
        if (lineRenderer == null) return;

        Vector3 startPos = handHoldPoint.position;
        Vector3 startVel = GetAimDirection() * currentThrowForce;

        lineRenderer.positionCount = arcResolution;

        for (int i = 0; i < arcResolution; i++)
        {
            float t = i * timeStep;
            Vector3 pos = startPos + startVel * t + 0.5f * Physics.gravity * t * t;
            lineRenderer.SetPosition(i, pos);
        }
    }

    // Helper: Get aim direction (right stick or forward if neutral)
    private Vector3 GetAimDirection()
    {
        Vector2 lookInput = assignedGamepad.rightStick.ReadValue();
        Vector3 lookDir = new Vector3(lookInput.x, 0f, lookInput.y);

        if (lookDir.sqrMagnitude > 0.1f)
        {
            return lookDir.normalized + Vector3.up * 0.5f;
        }
        else
        {
            return (transform.forward + Vector3.up * 0.5f).normalized;
        }
    }
}
