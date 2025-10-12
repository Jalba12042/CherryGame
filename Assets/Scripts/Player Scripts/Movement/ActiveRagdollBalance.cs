using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActiveRagdollController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerIndex = 0;
    private Gamepad assignedGamepad;

    [Header("Jump Settings")]
    public Transform feetPosition;
    public float groundCheckRadius = 0.5f;
    public LayerMask groundLayer;
    public float jumpForce = 80f;

    [Header("References")]
    public Transform targetPose; // Your upright reference pose
    public Rigidbody hips;       // Drag the pelvis Rigidbody here

    [Header("Arm Settings")]
    public CharacterJoint rightArmJoint;
    private Quaternion defaultArmTarget;
    private Quaternion liftedArmTarget;
    private bool holdingCherry;

    [Header("Balance Settings")]
    public float uprightForce = 2506f;
    public float uprightTorque = 8220f;

    [Header("Upright Stabilization")]
    public List<Rigidbody> uprightBodies; // Add spine, chest, head, etc.

    [Header("Movement Settings")]
    public float moveForce = 60f;   // Forward/backward/strafe force
    public float maxSpeed = 5f;     // Speed limit
    public float turnSpeed = 5f;    // How quickly the character rotates toward movement

    [Header("Throw Settings")]
    public Transform handHoldPoint;
    public GameObject cherryPrefab;
    private GameObject heldCherry;
    private GameObject nearbyCherry;
    private float pickupRadius = 6f;

    private Vector3 moveInput;

    void Start()
    {
        if (Gamepad.all.Count > playerIndex)
            assignedGamepad = Gamepad.all[playerIndex];

        StartCoroutine(InitializeArmTargets());

        if (rightArmJoint != null)
        {
            SoftJointLimit wideLimit = new SoftJointLimit { limit = 90f };
            rightArmJoint.swing1Limit = wideLimit;
            rightArmJoint.swing2Limit = wideLimit;
            rightArmJoint.lowTwistLimit = new SoftJointLimit { limit = -90f };
            rightArmJoint.highTwistLimit = new SoftJointLimit { limit = 90f };
        }
    }

    private IEnumerator InitializeArmTargets()
    {
        yield return new WaitForFixedUpdate();

        if (rightArmJoint != null)
        {
            Rigidbody armRb = rightArmJoint.GetComponent<Rigidbody>();
            if (armRb != null)
            {
                defaultArmTarget = armRb.rotation;
                liftedArmTarget = defaultArmTarget;
            }
        }
    }

    void Update()
    {
        // --- Movement input ---
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

        // --- Jump input ---
        bool isGrounded = Physics.CheckSphere(feetPosition.position, groundCheckRadius, groundLayer);

        if (assignedGamepad != null)
        {
            if (assignedGamepad.buttonSouth.wasPressedThisFrame && isGrounded)
            {
                Jump();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.J) && isGrounded)
            {
                Jump();
            }
        }

        // --- Detect nearby cherries ---
        Collider[] hits = Physics.OverlapSphere(hips.position, pickupRadius);
        nearbyCherry = null;
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Cherry"))
            {
                nearbyCherry = hit.gameObject;
                break;
            }
        }

        // --- Arm animation update ---
        if (rightArmJoint != null && holdingCherry)
        {
            Rigidbody armRb = rightArmJoint.GetComponent<Rigidbody>();
            if (armRb != null)
            {
                Quaternion deltaRot = liftedArmTarget * Quaternion.Inverse(armRb.rotation);
                deltaRot.ToAngleAxis(out float angle, out Vector3 axis);
                if (angle > 180f) angle -= 360f;

                if (Mathf.Abs(angle) > 0.1f)
                {
                    axis.Normalize();
                    float torqueStrength = Mathf.Min(angle * 50f, 100f);
                    armRb.AddTorque(axis * torqueStrength, ForceMode.Acceleration);
                }
            }
        }
    }

    private void Jump()
    {
        hips.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void PickUpCherry()
    {
        if (heldCherry == null && nearbyCherry != null)
        {
            heldCherry = nearbyCherry;
            Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
            if (rbCherry != null)
            {
                rbCherry.isKinematic = true;
                rbCherry.detectCollisions = false;
            }

            heldCherry.transform.position = handHoldPoint.position;
            heldCherry.transform.rotation = handHoldPoint.rotation;

            StartCoroutine(FollowHand());

            if (rightArmJoint != null)
            {
                Transform shoulder = rightArmJoint.transform;
                Vector3 liftOffset = shoulder.up * 1.5f + shoulder.forward * 0.5f;
                Vector3 targetPoint = handHoldPoint.position + liftOffset;
                liftedArmTarget = Quaternion.LookRotation(targetPoint - shoulder.position, Vector3.up);
            }

            holdingCherry = true;
        }
    }

    private IEnumerator FollowHand()
    {
        while (heldCherry != null)
        {
            heldCherry.transform.position = handHoldPoint.position;
            heldCherry.transform.rotation = handHoldPoint.rotation;
            yield return null;
        }
    }

    private void DropCherry()
    {
        if (heldCherry != null)
        {
            Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
            if (rbCherry != null)
            {
                rbCherry.isKinematic = false;
                rbCherry.detectCollisions = true;
            }

            heldCherry = null;
            holdingCherry = false;
            liftedArmTarget = defaultArmTarget;
        }
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

    void FixedUpdate()
    {
        if (hips == null || targetPose == null) return;

        KeepUpright();
        MoveCharacter();

        // Keep targetPose at hips position
        targetPose.position = hips.position;
    }

    private void KeepUpright()
    {
        Vector3 forceDir = targetPose.position - hips.position;
        hips.AddForce(forceDir * uprightForce * Time.fixedDeltaTime, ForceMode.Acceleration);

        Quaternion rotDiff = targetPose.rotation * Quaternion.Inverse(hips.rotation);
        rotDiff.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;
        axis.Normalize();

        float uprightDot = Vector3.Dot(hips.transform.up, Vector3.up);
        float recoveryMultiplier = Mathf.Lerp(1f, 3f, 1f - uprightDot);

        if (Mathf.Abs(angle) > 1f)
            hips.AddTorque(axis * angle * uprightTorque * recoveryMultiplier * Time.fixedDeltaTime, ForceMode.Acceleration);

        foreach (var rb in uprightBodies)
        {
            ApplyTorqueToMatchRotation(rb, targetPose.rotation, uprightTorque * 0.5f * recoveryMultiplier);
        }
    }

    private void ApplyTorqueToMatchRotation(Rigidbody rb, Quaternion targetRot, float torqueStrength)
    {
        Quaternion rotDiff = targetRot * Quaternion.Inverse(rb.rotation);
        rotDiff.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;
        axis.Normalize();

        if (Mathf.Abs(angle) > 1f)
            rb.AddTorque(axis * angle * torqueStrength * Time.fixedDeltaTime, ForceMode.Acceleration);
    }

    private void MoveCharacter()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 moveDir = moveInput.normalized;
            moveDir.y = 0;

            // Move hips
            Vector3 horizontalVelocity = new Vector3(hips.linearVelocity.x, 0, hips.linearVelocity.z);
            if (horizontalVelocity.magnitude < maxSpeed)
                hips.AddForce(moveDir * moveForce, ForceMode.Acceleration);

            // Rotate hips toward movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            hips.MoveRotation(Quaternion.Slerp(hips.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }
    }

}
