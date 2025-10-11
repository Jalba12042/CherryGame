using UnityEngine;
using UnityEngine.InputSystem;

public class ActiveRagdollController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerIndex = 0;
    private Gamepad assignedGamepad;

    [Header("References")]
    public Transform targetPose; // Your upright reference pose
    public Rigidbody hips;       // Drag the pelvis Rigidbody here

    [Header("Balance Settings")]
    public float uprightForce = 2506f;
    public float uprightTorque = 8220f;

    [Header("Movement Settings")]
    public float moveForce = 60f;   // Forward/backward/strafe force
    public float maxSpeed = 5f;     // Speed limit
    public float turnSpeed = 5f;    // How quickly the character rotates

    [Header("Throw Settings")]
    public Transform handHoldPoint;
    public GameObject cherryPrefab;
    private GameObject heldCherry;
    private bool isCharging;
    private GameObject nearbyCherry;
    public Projectile projectileScript;
    private float pickupRadius = 6f;

    private Vector3 moveInput;

    void Start()
    {
        if (Gamepad.all.Count > playerIndex)
            assignedGamepad = Gamepad.all[playerIndex];
    }


    void Update()
    {
        // Read movement input
        if (assignedGamepad != null)
        {
            Vector2 stick = assignedGamepad.leftStick.ReadValue();
            moveInput = new Vector3(stick.x, 0f, stick.y);

            float rtValue = assignedGamepad.rightTrigger.ReadValue();
            if (rtValue > 0.1f)
                PickUpCherry();
            else
                DropCherry();
        }
        else
        {
            moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            if (Input.GetKey(KeyCode.Space))
                PickUpCherry();
            else
                DropCherry();
        }

        // Detect nearby cherries manually
        Collider[] hits = Physics.OverlapSphere(hips.position, pickupRadius);
        nearbyCherry = null;
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Cherry"))
            {
                nearbyCherry = hit.gameObject;
                break; // pick the first one
            }
        }
    }

    private void PickUpCherry()
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

    private void DropCherry()
    {
        if (heldCherry != null && !isCharging) // Don't drop while charging a throw
        {
            Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
            heldCherry.transform.SetParent(null);
            if (rbCherry != null)
                rbCherry.isKinematic = false;

            heldCherry = null;
        }
    }

    // Detect nearby cherries
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


    void FixedUpdate()
    {
        if (hips == null || targetPose == null) return;

        KeepUpright();
        MoveCharacter();

        // Keep targetPose at hips position so rotation stays relative
        targetPose.position = hips.position;
    }

    private void KeepUpright()
    {
        // Position correction (optional)
        Vector3 forceDir = targetPose.position - hips.position;
        hips.AddForce(forceDir * uprightForce * Time.fixedDeltaTime, ForceMode.Acceleration);

        // Rotation correction
        Quaternion rotDiff = targetPose.rotation * Quaternion.Inverse(hips.rotation);
        rotDiff.ToAngleAxis(out float angle, out Vector3 axis);
        axis.Normalize();

        if (angle > 180f) angle -= 360f;
        if (Mathf.Abs(angle) > 1f)
        {
            hips.AddTorque(axis * angle * uprightTorque * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }

    private void MoveCharacter()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 moveDir = moveInput.normalized;

            // Keep movement horizontal
            moveDir.y = 0;

            // Current forward of hips
            Vector3 forward = hips.transform.forward;

            // Calculate the angle between current forward and desired direction
            float angle = Vector3.SignedAngle(forward, moveDir, Vector3.up);

            // Apply torque to rotate toward movement direction
            hips.AddTorque(Vector3.up * angle * turnSpeed, ForceMode.Acceleration);

            // Apply force to move forward if under max speed
            Vector3 horizontalVelocity = new Vector3(hips.linearVelocity.x, 0, hips.linearVelocity.z);
            if (horizontalVelocity.magnitude < maxSpeed)
            {
                hips.AddForce(moveDir * moveForce, ForceMode.Acceleration);
            }
        }
    }
}
