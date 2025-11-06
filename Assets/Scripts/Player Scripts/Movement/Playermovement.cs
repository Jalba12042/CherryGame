using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class Playermovement : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerIndex = 0; // Set this when the player is spawned
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Throw Settings")]
    public Transform handHoldPoint; // Where cherry sits
    public GameObject cherryPrefab; // Prefab for throwing

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Jump Tuning")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;


    [Header("Pickup Settings")]
    public Transform pickupTarget;      // Where the grabbed player should move to
    public float pickupRange = 2f;      // Range to detect other players
    public float grabCooldownTime = 1f; // seconds before player can grab again

    [Header("Powerup Settings")]
    //[SerializeField] private float powerupPickupRange = 2f;
    private Powerup nearbyPowerup;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 10f; // tweak this value in the Inspector



    public Gamepad assignedGamepad;
    private Rigidbody rb;
    private bool isGrounded;
    private bool jumpRequested = false;
    private Vector2 moveInput;

    private GameObject heldCherry;
    private bool isCharging;
    private GameObject nearbyCherry;

    private bool canGrab = true;
    private GameObject grabbedPlayer;
    private Rigidbody grabbedRigidbody;
    private bool isCurrentlyGrabbed = false;
    private Collider myCollider;
    private Collider grabbedCollider;


    public Projectile projectileScript;
    private Vector2 smoothLookInput;
    private Vector3 lastLookDir = Vector3.forward;

    // Big powerup screen shake
    [Header("While Big")]
    public bool isBig = false;
    [SerializeField] private float itemJumpForce = 7f;
    private bool wasGroundedLastFrame = false;

    // Screen Shake reference
    [Header("Screen Shake")]
    [SerializeField] private ScreenShake screenShake;

    [Header("Current Powerups activated")]
    public List<bool> currPowerups;
    public Dictionary<int, Powerup> activePowerupInstances = new Dictionary<int, Powerup>();

    // --- Sprinkler Interaction Fields ---
    [Header("Sprinkler Interaction")]
    [Tooltip("How strongly the sprinkler pushes the player back.")]
    public float pushbackResistance = 1f; // higher = less push
    [Tooltip("If true, player is currently slowed by water.")]
    public bool IsSlowed = false;
    private float originalMoveSpeed;

    [Header("Animator")]
    private Animator animator;


    void Start()
    {
        animator = GetComponent<Animator>();

        screenShake = FindFirstObjectByType<ScreenShake>();
        /*if (Gamepad.all.Count > playerIndex)
            assignedGamepad = Gamepad.all[playerIndex];*/

        rb = GetComponent<Rigidbody>();
        projectileScript = GetComponent<Projectile>();

        if (projectileScript != null)
            projectileScript.SetOwner(this);

        originalMoveSpeed = moveSpeed;

        myCollider = GetComponent<Collider>();

        int highestID = -1;
        for (int i = 0; i < RoundManager.Instance.powerUpsInRotation.Count; i++)
        {
            if (RoundManager.Instance.powerUpsInRotation[i].GetComponent<Powerup>().powerUpID > highestID)
            {
                highestID = RoundManager.Instance.powerUpsInRotation[i].GetComponent<Powerup>().powerUpID;
            }
        }

        if (highestID >= 0)
        {
            currPowerups = new List<bool>();
            for (int i = 0; i <= highestID; i++)
            {
                currPowerups.Add(false);
            }
        }
        else
        {
            currPowerups = null;
        }
    }

    private void FixedUpdate()
    {
        // --- Movement (Left Stick) ---
        moveInput = assignedGamepad != null ? assignedGamepad.leftStick.ReadValue() : Vector2.zero;
        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, groundCheckDistance, groundLayer);

        if (!wasGroundedLastFrame && isGrounded && isBig)
        {
            if (screenShake != null)
            {
                // shake screen
                screenShake.Shake();
            }
            if (RoundManager.Instance.currRound.goalObjects != null)
            {
                // send goal objects flying
                for (int i = 0; i < RoundManager.Instance.currRound.goalObjects.Count; i++)
                {
                    ItemGroundCheck groundCheck = RoundManager.Instance.currRound.goalObjects[i].GetComponent<ItemGroundCheck>();
                    Rigidbody rb = RoundManager.Instance.currRound.goalObjects[i].GetComponent<Rigidbody>();

                    if (groundCheck != null && rb != null)
                    {
                        rb.AddForce(Vector3.up * itemJumpForce, ForceMode.Impulse);
                    }
                }
            }
            if (RoundManager.Instance.playerObjects != null)
            {
                // send other players flying
                for (int i = 0; i < RoundManager.Instance.playerObjects.Length; i++)
                {
                    if (i != playerIndex)
                    {
                        Playermovement pm = RoundManager.Instance.playerObjects[i].GetComponent<Playermovement>();

                        if (pm != null)
                        {
                            Rigidbody rb = RoundManager.Instance.playerObjects[i].GetComponent<Rigidbody>();

                            if (rb != null && pm.isGrounded)
                            {
                                rb.AddForce(Vector3.up * itemJumpForce, ForceMode.Impulse);
                            }
                        }
                    }
                }
            }
            if (RoundManager.Instance.powerupsInPlay != null)
            {
                if (RoundManager.Instance.powerupsInPlay.Count != 0)
                {
                    // send powerups flying
                    for (int i = 0; i < RoundManager.Instance.powerupsInPlay.Count; i++)
                    {
                        ItemGroundCheck groundCheck = RoundManager.Instance.powerupsInPlay[i].GetComponent<ItemGroundCheck>();
                        Rigidbody rb = RoundManager.Instance.powerupsInPlay[i].GetComponent<Rigidbody>();

                        if (groundCheck != null && rb != null)
                        {
                            rb.AddForce(Vector3.up * itemJumpForce, ForceMode.Impulse);
                        }
                    }
                }
            }
        }

        wasGroundedLastFrame = isGrounded;
        //Debug.DrawRay(groundCheckPoint.position, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);


        if (isGrounded && moveInput == Vector2.zero)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        Vector3 targetVelocity = move * moveSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        if (animator != null)
        {
            float speedValue = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
            animator.SetFloat("Speed", speedValue);
        }


        /*Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        if (move.magnitude > 0.1f)
        {
            Vector3 moveDir = transform.TransformDirection(move.normalized);
            Vector3 targetVelocity = moveDir * moveSpeed;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }*/


        // --- Jump (Button South) ---
        if (jumpRequested && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // reset vertical
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            jumpRequested = false;
        }

        // --- Improved Jump Gravity ---
        if (!isGrounded)
        {
            // Falling down: apply extra gravity for faster fall
            if (rb.linearVelocity.y < 0)
            {
                rb.AddForce(Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * rb.mass);
            }
            // Rising but jump button released: apply smaller gravity boost
            else if (rb.linearVelocity.y > 0 && !assignedGamepad.buttonSouth.isPressed)
            {
                rb.AddForce(Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * rb.mass);
            }
        }


    }

    void Update()
    {
        if (assignedGamepad == null || rb == null)
            return;

        // --- Jump Input ---
        if (isGrounded && assignedGamepad.buttonSouth.wasPressedThisFrame)
        {
            //Debug.Log($"Player {playerIndex} jumped!");
            jumpRequested = true;
        }
        // --- Rotation (Right Stick with Smoothed Deadzone) ---
        Vector2 rawLook = assignedGamepad.rightStick.ReadValue();
        float magnitude = rawLook.magnitude;

        if (magnitude > 0.05f) // ignore micro noise
        {
            // Convert stick direction into camera-relative world direction
            Transform cam = Camera.main.transform;
            Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 camRight = Vector3.Scale(cam.right, new Vector3(1, 0, 1)).normalized;

            Vector3 lookDir = (camRight * rawLook.x + camForward * rawLook.y).normalized;

            // Smooth magnitude response instead of hard cutoff
            float inputDeadzone = 0.2f;
            float smoothedMagnitude = Mathf.InverseLerp(inputDeadzone, 1f, magnitude);

            // Smoothly rotate toward look direction
            Quaternion targetRotation = Quaternion.LookRotation(lookDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(
    transform.rotation,
    targetRotation,
    Time.deltaTime * rotationSpeed * smoothedMagnitude
);


            lastLookDir = lookDir;
        }



        // --- Pickup / Drop Cherry (RT) ---
        float rtValue = assignedGamepad.rightTrigger.ReadValue();
        if (rtValue > 0.1f)
        {
            HandleCherryPickup();
            HandlePlayerGrab();
            HandlePowerupPickup();
        }
        else
        {
            HandleCherryDrop();
            HandlePlayerRelease();
        }
    }

    // --- Handle Player Pickup Logic ---
    private void HandlePlayerGrab()
    {
        if (grabbedPlayer == null && canGrab)
        {
            GameObject nearby = GetNearbyPlayer();
            if (nearby != null)
            {
                grabbedPlayer = nearby;
                grabbedRigidbody = grabbedPlayer.GetComponent<Rigidbody>();

                if (grabbedRigidbody != null)
                    grabbedRigidbody.isKinematic = true;

                grabbedCollider = grabbedPlayer.GetComponent<Collider>();
                if (grabbedCollider != null && myCollider != null)
                    Physics.IgnoreCollision(myCollider, grabbedCollider, true);

                // Assign grabber reference
                PlayerGrabbed grabbed = grabbedPlayer.GetComponent<PlayerGrabbed>();
                if (grabbed != null)
                    grabbed.grabber = this;

                // Temporarily disable movement on grabbed player
                Playermovement grabbedMove = grabbedPlayer.GetComponent<Playermovement>();
                if (grabbedMove != null)
                    grabbedMove.enabled = false;

                // Trigger escape UI
                PlayerEscapeUI escapeUI = grabbedPlayer.GetComponentInChildren<PlayerEscapeUI>();
                if (escapeUI != null)
                    escapeUI.StartBeingGrabbed(grabbedMove.playerIndex);

                isCurrentlyGrabbed = true;
            }
        }

        if (grabbedPlayer != null && pickupTarget != null)
        {
            grabbedPlayer.transform.position = pickupTarget.position;
            grabbedPlayer.transform.rotation = pickupTarget.rotation;
        }
    }

    public void HandlePlayerRelease()
    {
        if (grabbedPlayer != null)
        {
            if (grabbedRigidbody != null)
                grabbedRigidbody.isKinematic = false;

            if (grabbedCollider != null && myCollider != null)
                Physics.IgnoreCollision(myCollider, grabbedCollider, false);
            grabbedCollider = null;

            if (isCurrentlyGrabbed)
            {
                Playermovement grabbedMove = grabbedPlayer.GetComponent<Playermovement>();
                if (grabbedMove != null)
                    grabbedMove.enabled = true;

                PlayerEscapeUI escapeUI = grabbedPlayer.GetComponentInChildren<PlayerEscapeUI>();
                if (escapeUI != null)
                    escapeUI.StopBeingGrabbed();

                isCurrentlyGrabbed = false;
            }

            grabbedPlayer = null;
            grabbedRigidbody = null;

            StartCoroutine(GrabCooldown());
        }
    }

    private GameObject GetNearbyPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange);
        foreach (var hit in hits)
        {
            Playermovement pc = hit.GetComponent<Playermovement>();
            if (pc == null || pc.playerIndex == playerIndex) continue;
            return hit.gameObject;
        }
        return null;
    }

    private void HandlePowerupPickup()
    {
        if (nearbyPowerup != null)
        {
            nearbyPowerup.Activate(this);
            nearbyPowerup = null; // prevent re-trigger
        }
    }

    public IEnumerator GrabCooldown()
    {
        canGrab = false;
        yield return new WaitForSeconds(grabCooldownTime);
        canGrab = true;
    }

    // --- Handle Cherry Logic ---
    private void HandleCherryPickup()
    {
        // Don't pickup while aiming or throwing
        if (projectileScript != null && projectileScript.IsAiming())
            return;

        if (heldCherry == null && nearbyCherry != null)
        {
            heldCherry = nearbyCherry;
            Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
            if (rbCherry != null) rbCherry.isKinematic = true;
            heldCherry.transform.SetParent(handHoldPoint);
            heldCherry.transform.localPosition = Vector3.zero;
            if (projectileScript != null) projectileScript.PickUpCherry(heldCherry);
        }
    }


    private void HandleCherryDrop()
    {
        if (heldCherry != null && !isCharging)
        {
            Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
            heldCherry.transform.SetParent(null);
            if (rbCherry != null) rbCherry.isKinematic = false;

            // Tell Projectile to cancel aiming/throwing
            if (projectileScript != null)
            {
                projectileScript.CancelAim();
            }

            heldCherry = null;
        }
    }




    // Pick up cherry
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cherry"))
        {
            nearbyCherry = other.gameObject;
        }

        // Detect powerup
        if (other.TryGetComponent(out Powerup powerup))
        {
            nearbyPowerup = powerup;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Cherry") && other.gameObject == nearbyCherry)
        {
            nearbyCherry = null;
        }

        // Lose reference when leaving range
        if (other.TryGetComponent(out Powerup powerup) && powerup == nearbyPowerup)
        {
            nearbyPowerup = null;
        }
    }

    public Gamepad GetAssignedGamepad() => assignedGamepad;

    // --- Sprinkler Interaction API (called from SprinklerInteraction.cs) ---

    public void ApplyPushback(Vector3 direction, float force)
    {
        if (rb == null) return;
        Vector3 push = direction.normalized * (force / pushbackResistance);
        rb.AddForce(push, ForceMode.VelocityChange);
    }

    public void ApplySlow(float slowMultiplier, float duration)
    {
        if (IsSlowed) return; // prevent stacking

        StartCoroutine(SlowRoutine(slowMultiplier, duration));
    }

    private IEnumerator SlowRoutine(float slowMultiplier, float duration)
    {
        IsSlowed = true;
        moveSpeed *= slowMultiplier;
        yield return new WaitForSeconds(duration);
        moveSpeed = originalMoveSpeed;
        IsSlowed = false;
    }

    /*public void InitializePlayer(int index)
    {
        playerIndex = index;
        if (Gamepad.all.Count > playerIndex)
        {
            // adjust playerIndex on each script
            GetComponentInChildren<PlayerEscapeUI>().playerIndex = playerIndex;

            // assign gamepad
            assignedGamepad = Gamepad.all[playerIndex];
            Debug.Log($"Player {playerIndex + 1} initialized with gamepad: {assignedGamepad.displayName}");
        }
        else
        {
            Debug.LogWarning($"Player {playerIndex + 1} could not find a gamepad!");
        }
    }*/
}
