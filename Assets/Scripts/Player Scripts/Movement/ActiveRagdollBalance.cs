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
    public Transform PlayerRoot; // new root object
    public Transform PlayerRigged; // visuals
    public Rigidbody hips; // physics body
    public Transform targetPose; // Your upright reference pose

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
    private Vector2 lookInput;

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
        if (assignedGamepad != null)
        {
            // Left stick = move
            Vector2 moveStick = assignedGamepad.leftStick.ReadValue();
            moveInput = new Vector3(moveStick.x, 0f, moveStick.y);

            // Right stick = look
            lookInput = assignedGamepad.rightStick.ReadValue();

            float rtValue = assignedGamepad.rightTrigger.ReadValue();
            if (rtValue > 0.1f)
                PickUpCherry();
            else
                DropCherry();
        }
        else
        {
            // Keyboard fallback
            moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            lookInput = Vector2.zero;

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

        // --- Arm animation ---
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
        if (hips == null || PlayerRoot == null) return;

        // --- Keep upright ---
        KeepUpright();

        // --- Move character ---
        MoveCharacter();

        // --- Smoothly sync visuals ---
        float followSpeed = 10f;
        PlayerRoot.position = Vector3.Lerp(PlayerRoot.position, hips.position, followSpeed * Time.fixedDeltaTime);
        PlayerRoot.rotation = Quaternion.Slerp(PlayerRoot.rotation, hips.rotation, followSpeed * Time.fixedDeltaTime);
    }


    /*void LateUpdate()
    {
        if (hips == null) return;

        // Smoothly sync PlayerRigged position and rotation with hips
        transform.position = Vector3.Lerp(
            transform.position,
            hips.position,
            15f * Time.deltaTime
        );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            hips.rotation,
            15f * Time.deltaTime
        );
    }*/



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
        Vector3 moveDir = moveInput;
        if (moveDir.sqrMagnitude > 0.01f)
        {
            moveDir.Normalize();

            // Current horizontal velocity
            Vector3 horizontalVel = new Vector3(hips.linearVelocity.x, 0f, hips.linearVelocity.z);

            // Apply force only if below max speed
            if (horizontalVel.magnitude < maxSpeed)
                hips.AddForce(moveDir * moveForce, ForceMode.Force);

            // Clamp horizontal speed
            horizontalVel = new Vector3(hips.linearVelocity.x, 0f, hips.linearVelocity.z);
            if (horizontalVel.magnitude > maxSpeed)
            {
                Vector3 clamped = horizontalVel.normalized * maxSpeed;
                hips.linearVelocity = new Vector3(clamped.x, hips.linearVelocity.y, clamped.z);
            }
        }

        // Rotation based on right stick
        if (lookInput.sqrMagnitude > 0.1f)
        {
            Vector3 lookDir = new Vector3(lookInput.x, 0f, lookInput.y);
            Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
            hips.MoveRotation(Quaternion.Slerp(hips.rotation, targetRot, turnSpeed * Time.fixedDeltaTime));
        }
    }





}
