using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Merged from: PlayerGrab, PlayerCherry, PlayerGrabbed
// One RT press attempts, in priority order: release, grab player, pick up cherry.
// LT release throws a held player. Cherry throw is handled by Projectile (hold/release LT).
public class PlayerInteract : MonoBehaviour
{
    // ===== GRAB =====
    [Header("Grab Settings")]
    public float pickupRange = 2f;

    [Tooltip("How fast the grabbed player moves while being grabbed by one player.")]
    [Range(0f, 1f)]
    public float grabbedSpeedMultiplier = 0.35f;

    [Tooltip("How long a grabber cannot grab the same player after that player escapes.")]
    public float escapeCooldownTime = 1f;

    [Tooltip("How long a normal grab cooldown lasts after releasing someone.")]
    public float grabCooldownTime = 1f;

    [Header("Grab Point")]
    [SerializeField] private Transform grabPoint;

    [Header("Grab Connection")]
    [SerializeField] private float grabConnectionDistance = 0.8f;

    [SerializeField] private float grabConnectionStrength = 25f;

    [SerializeField] private float grabConnectionDamping = 8f;

    [SerializeField] private float grabberPriority = 1.25f;

    [Header("Locked Grab Side")]
    [SerializeField] private float lockedGrabOffset = 0.8f;
    [SerializeField] private float lockedGrabStrength = 30f;
    [SerializeField] private float lockedGrabDamping = 10f;

    private class GrabData
    {
        public PlayerInteract target;
        public Vector3 localGrabOffset;
        public string lockedSide;
    }

    private readonly List<GrabData> grabData = new List<GrabData>();


    [Header("Grab Audio")]
    [SerializeField] private AudioSource grabSource;
    [SerializeField] private AudioClip grabClip;
    [SerializeField] private AudioClip grabDropClip;

    [Header("Face Animator")]
    [SerializeField] private Animator faceAnimator;

    // ===== CHERRY =====
    [Header("Pickup")]
    public Transform handHoldPoint;
    [SerializeField] private float pickupRadius = 3f;

    [Header("Cherry Audio")]
    [SerializeField] private AudioSource pickupSource;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private AudioClip cherryDropClip;

    // ===== GRABBED STATE (was PlayerGrabbed) =====
    // Players THIS player is currently grabbing
    private readonly List<PlayerInteract> grabbedPlayers = new List<PlayerInteract>();

    // Players currently grabbing THIS player
    private readonly List<PlayerInteract> grabbingPlayers = new List<PlayerInteract>();

    // Players this player currently cannot grab because of an escape cooldown
    private readonly Dictionary<PlayerInteract, Coroutine> escapeCooldowns =
        new Dictionary<PlayerInteract, Coroutine>();

    private readonly Dictionary<PlayerInteract, Vector3> grabDirections =
    new Dictionary<PlayerInteract, Vector3>();


    // ===== PRIVATE =====
    private Playermovement player;
    private Projectile projectileScript;
    private SnowballThrow snowballThrow;
    private Animator animator;
    private PlayerPowerupHandler powerupHandler;
    private Collider myCollider;

    private bool canGrab = true;

    public bool IsBeingGrabbed => grabbingPlayers.Count > 0;

    public bool IsGrabbingSomeone => grabbedPlayers.Count > 0;

    public int NumberOfGrabbers => grabbingPlayers.Count;

    private GameObject nearbyPlayer;

    // SNOWBALL STUFF
    private SnowballPile nearbySnowPile;
    [SerializeField] private GameObject snowballPrefab;

    private int snowballsRemaining = 0;

    private bool rtWasDown = false;
    private float rtHoldTime = 0f;
    private bool ignoreNextRelease = false;

    [SerializeField]
    private float throwHoldThreshold = 0.2f;

    private GameObject heldPickup;
    private float cherryPickupCooldown = 0f;

    private void Awake()
    {
        if (faceAnimator == null)
            faceAnimator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        player = GetComponent<Playermovement>();
        powerupHandler = GetComponent<PlayerPowerupHandler>();
        projectileScript = GetComponent<Projectile>();
        animator = GetComponent<Animator>();
        myCollider = GetComponent<Collider>();
        snowballThrow = GetComponent<SnowballThrow>();
    }

    void Update()
    {
        if (cherryPickupCooldown > 0f)
            cherryPickupCooldown -= Time.deltaTime;

        if (!canGrab)
            return;

        bool rt = InputManager.Instance.GetButton1Held(player.playerID);

        bool rtPressed = rt && !rtWasDown;
        bool rtReleased = !rt && rtWasDown;

        // PICKUP / GRAB IMMEDIATELY ON PRESS
        if (rtPressed)
        {
            // Only pick up/grab if we are currently empty-handed
            if (heldPickup == null)
            {
                OnInteractPressed();

                // Only use the pickup release protection if
                // we actually picked up an item.
                if (heldPickup != null)
                {
                    ignoreNextRelease = true;
                }
            }

            rtHoldTime = 0f;
        }

        /*if (rtPressed)
        {
            // Only pick up/grab if we are currently empty-handed
            if (heldPickup == null)
            {
                OnInteractPressed();

                // Prevent the release of this same button press
                // from immediately dropping the newly picked-up item.
                ignoreNextRelease = true;
            }

            rtHoldTime = 0f;
        }*/

        // TRACK HOLD TIME
        if (rt)
        {
            rtHoldTime += Time.deltaTime;
        }

        // HANDLE RELEASE
        if (rtReleased)
        {
            // =====================================================
            // PLAYER GRAB RELEASE
            // =====================================================

            // If we are grabbing a player, releasing RT releases them.
            // This happens before pickup logic so it does not interfere
            // with cherries/snowballs.
            if (IsGrabbingSomeone)
            {
                ReleaseAllGrabbedPlayers();

                rtHoldTime = 0f;
                rtWasDown = rt;
                return;
            }


            // =====================================================
            // LEVEL PICKUP LOGIC
            // =====================================================

            // This is the release of the RT press that picked up
            // the item. Keep the existing protection.
            if (ignoreNextRelease)
            {
                ignoreNextRelease = false;
                rtHoldTime = 0f;
                rtWasDown = rt;
                return;
            }

            bool wasHold = rtHoldTime >= throwHoldThreshold;

            if (!wasHold)
            {
                // Tap while already holding a pickup = drop it
                if (heldPickup != null)
                {
                    LevelPickup pickup =
                        heldPickup.GetComponent<LevelPickup>();

                    if (pickup != null && pickup.useProjectileThrow)
                    {
                        // Cherry
                        if (!projectileScript.IsAiming())
                        {
                            CancelAimAndDrop();
                        }
                    }
                    else
                    {
                        // Snowball
                        snowballThrow.ThrowSnowball();
                    }
                }
            }

            // If this was a hold, Projectile handles the throw.
            rtHoldTime = 0f;
        }



        /*if (rtReleased)
        {
            // This is the release of the RT press that picked up the item.
            // Ignore it so the item does not instantly drop.
            if (ignoreNextRelease)
            {
                ignoreNextRelease = false;
                rtHoldTime = 0f;
                rtWasDown = rt;
                return;
            }

            bool wasHold = rtHoldTime >= throwHoldThreshold;

            if (!wasHold)
            {
                // Tap while already holding a pickup = drop it
                if (heldPickup != null)
                {
                    LevelPickup pickup =
                        heldPickup.GetComponent<LevelPickup>();

                    if (pickup != null && pickup.useProjectileThrow)
                    {
                        // Cherry
                        if (!projectileScript.IsAiming())
                        {
                            CancelAimAndDrop();
                        }
                    }
                    else
                    {
                        // Snowball
                        snowballThrow.ThrowSnowball();
                    }
                }
            }

            // IMPORTANT:
            // If this was a hold, do nothing here.
            // Projectile handles the actual throw when RT is released.
            rtHoldTime = 0f;
    }*/

        rtWasDown = rt;
    }

    private void FixedUpdate()
    {
        UpdateGrabbedPlayers();

        if (IsBeingGrabbed)
        {
            UpdateGrabbedPullAnimation();
        }
    }

    private void OnInteractPressed()
    {
        if (nearbySnowPile != null && snowballsRemaining == 0)
        {
            GiveSnowballs();
            return;
        }

        if (heldPickup != null)
        {
            LevelPickup pickup = heldPickup.GetComponent<LevelPickup>();

            if (pickup != null && pickup.useProjectileThrow)
            {
                // Cherry
                if (!projectileScript.IsAiming())
                    CancelAimAndDrop();
            }
            else
            {
                // Snowball
                snowballThrow.ThrowSnowball();
            }

            return;
        }
        if (TryGrab()) return;
        HandlePickup();
    }

    // =================================== START OF PLAYER GRABBING ==========================
    public bool TryGrab()
    {
        if (!canGrab)
            return false;

        nearbyPlayer = FindClosestPlayer();

        if (nearbyPlayer == null)
            return false;

        PlayerInteract targetInteract =
            nearbyPlayer.GetComponent<PlayerInteract>();

        if (targetInteract == null)
            return false;

        // Cannot grab yourself
        if (targetInteract == this)
            return false;

        // Cannot grab someone who is on cooldown specifically from you
        if (escapeCooldowns.ContainsKey(targetInteract))
            return false;

        PlayerEffects pe =
            nearbyPlayer.GetComponent<PlayerEffects>();

        if (pe != null && pe.isBig)
            return false;
        
        //Adds this player as a grabber
        if (grabbedPlayers.Contains(targetInteract))
            return false;


        grabbedPlayers.Add(targetInteract);

        targetInteract.AddGrabber(this);

        // Remember exactly which side we grabbed them from.
        Vector3 grabDirection =
            transform.position - targetInteract.transform.position;

        grabDirection.y = 0f;

        if (grabDirection.sqrMagnitude > 0.01f)
        {
            // Convert the grabber's position into the grabbed player's
            // local space so we know which side they are on.
            Vector3 localDirection =
                targetInteract.transform.InverseTransformDirection(
                    grabDirection.normalized
                );

            string lockedSide;

            if (Mathf.Abs(localDirection.x) > Mathf.Abs(localDirection.z))
            {
                lockedSide = localDirection.x > 0f ? "RIGHT" : "LEFT";
            }
            else
            {
                lockedSide = localDirection.z > 0f ? "BACK" : "FRONT";
            }

            Debug.Log("Player is grabbing " + lockedSide + " side");

            GrabData data = new GrabData();

            data.target = targetInteract;
            data.lockedSide = lockedSide;

            // Store the grabber's position relative to the grabbed player's rotation.
            data.localGrabOffset =
                localDirection * lockedGrabOffset;

            grabData.Add(data);
        }


        if (animator != null)
            animator.SetBool("isGrabbing", true);

        
        if (grabSource != null && grabClip != null)
        {
            grabSource.pitch = Random.Range(0.95f, 1.05f);
            grabSource.PlayOneShot(grabClip);
        }

        return true;
    }

    private void UpdateGrabbedPlayers()
    {
        if (grabData.Count > 0)
        {
            GrabData debugData = grabData[0];

            if (debugData != null && debugData.target != null)
            {
                Vector3 currentDirection =
                    transform.position - debugData.target.transform.position;

                currentDirection.y = 0f;

                if (currentDirection.sqrMagnitude > 0.01f)
                {
                    Vector3 localCurrentDirection =
                        debugData.target.transform.InverseTransformDirection(
                            currentDirection.normalized
                        );

                    string currentSide;

                    if (Mathf.Abs(localCurrentDirection.x) >
                        Mathf.Abs(localCurrentDirection.z))
                    {
                        currentSide =
                            localCurrentDirection.x > 0f
                            ? "RIGHT"
                            : "LEFT";
                    }
                    else
                    {
                        currentSide =
                            localCurrentDirection.z > 0f
                            ? "FRONT"
                            : "BACK";
                    }

                    Debug.Log(
                        "Locked side: " +
                        debugData.lockedSide +
                        " | Current side: " +
                        currentSide
                    );
                }
            }
        }


        if (grabData.Count == 0)
            return;

        foreach (GrabData data in grabData)
        {
            if (data == null || data.target == null)
                continue;

            PlayerInteract grabbedPlayer = data.target;

            Rigidbody grabberRigidbody =
                GetComponent<Rigidbody>();

            Rigidbody grabbedRigidbody =
                grabbedPlayer.GetComponent<Rigidbody>();

            if (grabberRigidbody == null ||
                grabbedRigidbody == null)
                continue;


            // =====================================================
            // FIND WHERE THE GRABBER SHOULD BE
            // =====================================================

            Vector3 desiredOffset =
                grabbedPlayer.transform.TransformDirection(
                    data.localGrabOffset
                );

            desiredOffset.y = 0f;

            Vector3 desiredPosition =
                grabbedPlayer.transform.position +
                desiredOffset;


            // =====================================================
            // HOW FAR IS THE GRABBER FROM ITS LOCKED POSITION?
            // =====================================================

            Vector3 positionError =
                desiredPosition -
                transform.position;

            positionError.y = 0f;


            // =====================================================
            // RELATIVE VELOCITY
            // =====================================================

            Vector3 grabberVelocity =
                grabberRigidbody.linearVelocity;

            Vector3 grabbedVelocity =
                grabbedRigidbody.linearVelocity;

            grabberVelocity.y = 0f;
            grabbedVelocity.y = 0f;

            Vector3 relativeVelocity =
                grabbedVelocity -
                grabberVelocity;


            // =====================================================
            // SPRING + DAMPING
            // =====================================================

            float separatingVelocity =
                Vector3.Dot(
                    relativeVelocity,
                    positionError.normalized
                );


            float springForce =
                positionError.magnitude *
                lockedGrabStrength;


            float dampingForce =
                separatingVelocity *
                lockedGrabDamping;


            float totalForce =
                springForce +
                dampingForce;


            Vector3 force;

            if (positionError.sqrMagnitude > 0.0001f)
            {
                force =
                    positionError.normalized *
                    totalForce;
            }
            else
            {
                force = Vector3.zero;
            }


            // =====================================================
            // BOTH PLAYERS PARTICIPATE IN THE CONNECTION
            // =====================================================

            // Grabber gets pulled toward the locked position.
            grabberRigidbody.AddForce(
                force,
                ForceMode.Acceleration
            );

            // Grabbed player gets pulled along with the grabber.
            grabbedRigidbody.AddForce(
                -force * grabberPriority,
                ForceMode.Acceleration
            );
        }
    }

    private void UpdateGrabbedPullAnimation()
    {
        // This player isn't being grabbed.
        if (grabbingPlayers.Count == 0)
        {
            animator.SetBool("isPullingLeft", false);
            animator.SetBool("isPullingRight", false);
            animator.SetBool("isPullingBoth", false);
            return;
        }

        bool hasLeftGrabber = false;
        bool hasRightGrabber = false;

        foreach (PlayerInteract grabber in grabbingPlayers)
        {
            if (grabber == null)
                continue;

            Vector3 direction =
                grabber.transform.position - transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                continue;

            // Convert grabber position into THIS player's local space.
            Vector3 localDirection =
                transform.InverseTransformDirection(
                    direction.normalized
                );

            // Ignore front/back for these animations.
            if (Mathf.Abs(localDirection.x) >
                Mathf.Abs(localDirection.z))
            {
                if (localDirection.x > 0f)
                    hasRightGrabber = true;
                else
                    hasLeftGrabber = true;
            }
        }

        // LEFT + RIGHT
        animator.SetBool(
            "isPullingBoth",
            hasLeftGrabber && hasRightGrabber
        );

        // LEFT only
        animator.SetBool(
            "isPullingLeft",
            hasLeftGrabber && !hasRightGrabber
        );

        // RIGHT only
        animator.SetBool(
            "isPullingRight",
            hasRightGrabber && !hasLeftGrabber
        );
    }

    /*private void UpdateGrabbedPlayers()
    {
        if (grabbedPlayers.Count == 0)
            return;

        foreach (PlayerInteract grabbedPlayer in grabbedPlayers)
        {
            if (grabbedPlayer == null)
                continue;

            Rigidbody grabberRigidbody =
                GetComponent<Rigidbody>();

            Rigidbody grabbedRigidbody =
                grabbedPlayer.GetComponent<Rigidbody>();

            if (grabberRigidbody == null ||
                grabbedRigidbody == null)
                continue;


            //Gets connection direction
            Vector3 connection =
                grabbedPlayer.transform.position -
                transform.position;

            connection.y = 0f;

            float distance =
                connection.magnitude;

            if (distance <= 0.01f)
                continue;

            Vector3 direction =
                connection.normalized;

            //Determines ideal distance for both players
            float distanceError =
                distance -
                grabConnectionDistance;
  
            
            //Gets both players horizontal vertices
            Vector3 grabberVelocity =
                grabberRigidbody.linearVelocity;

            Vector3 grabbedVelocity =
                grabbedRigidbody.linearVelocity;

            grabberVelocity.y = 0f;
            grabbedVelocity.y = 0f;


            Vector3 relativeVelocity =
                grabbedVelocity -
                grabberVelocity;


            // How quickly they are moving away from each other
            float separatingVelocity =
                Vector3.Dot(
                    relativeVelocity,
                    direction);

            //Spring Force
            float springForce =
                distanceError *
                grabConnectionStrength;

            //Damping
            float dampingForce =
                separatingVelocity *
                grabConnectionDamping;


            float totalForce =
                springForce +
                dampingForce;


          
            //Applies force to both players
            Vector3 force =
                direction *
                totalForce;


            // Grabbed player is pulled toward grabber
            grabbedRigidbody.AddForce(
                -force,
                ForceMode.Acceleration);


            // Grabber is pulled toward grabbed player
            grabberRigidbody.AddForce(
                force *
                grabberPriority,
                ForceMode.Acceleration);
        }
    }*/

    private GameObject FindClosestPlayer()
    {
        Collider[] hits =
            Physics.OverlapSphere(transform.position, pickupRange);

        GameObject closestPlayer = null;
        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            Playermovement pm =
                hit.GetComponent<Playermovement>() ??
                hit.GetComponentInParent<Playermovement>();

            if (pm == null)
                continue;

            if (pm.playerIndex == player.playerIndex)
                continue;

            PlayerInteract targetInteract =
                pm.GetComponent<PlayerInteract>();

            if (targetInteract == null)
                continue;

            // Do not grab a player who is currently on cooldown from us
            if (escapeCooldowns.ContainsKey(targetInteract))
                continue;

            float distance =
                Vector3.Distance(transform.position, pm.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = pm.gameObject;
            }
        }

        return closestPlayer;
    }


    //Is called on player being grabbed
    public void AddGrabber(PlayerInteract grabber)
    {
        if (grabber == null)
            return;

        if (grabbingPlayers.Contains(grabber))
            return;

        grabbingPlayers.Add(grabber);
        
        //Applies Slowdown
        if (grabbingPlayers.Count == 1)
        {
            StartCoroutine(ApplyGrabSlowdown());


            // Start escape UI
            PlayerEscapeUI escapeUI =
                GetComponent<PlayerEscapeUI>();

            if (escapeUI != null)
                escapeUI.StartBeingGrabbed();
        }

        
        //Grabbed animation
        if (animator != null)
            animator.SetBool("isGrabbed", true);
    }


    public void RemoveGrabber(PlayerInteract grabber)
    {
        if (grabber == null)
            return;

        grabbingPlayers.Remove(grabber);


        
        //if no one is grabbing anyone
        if (grabbingPlayers.Count == 0)
        {
            StopBeingGrabbed();

            PlayerEscapeUI escapeUI =
                GetComponent<PlayerEscapeUI>();

            if (escapeUI != null)
                escapeUI.StopBeingGrabbed();
        }
    }


    //Grabbed player is slow after being released/Escaping
    private IEnumerator ApplyGrabSlowdown()
    {
        float originalSpeed = player.moveSpeed;

        while (grabbingPlayers.Count > 0)
        {
            int grabberCount = grabbingPlayers.Count;


            // One grabber = slowed
            if (grabberCount == 1)
            {
                player.moveSpeed =
                    originalSpeed * grabbedSpeedMultiplier;
            }


            // Two or more grabbers = cannot move
            else
            {
                player.moveSpeed = 0f;
            }

            yield return null;
        }

        // Gradually restore speed over 3 seconds
        float startingSpeed = player.moveSpeed;
        float elapsed = 0f;

        while (elapsed < 3f)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / 3f;

            player.moveSpeed =
                Mathf.Lerp(startingSpeed, originalSpeed, t);

            yield return null;
        }

        player.moveSpeed = originalSpeed;
    }


    private void StopBeingGrabbed()
    {
        if (animator != null)
        {
            animator.SetBool("isGrabbed", false);

            animator.SetBool("isPullingLeft", false);
            animator.SetBool("isPullingRight", false);
            animator.SetBool("isPullingBoth", false);
        }
    }

    public void ReleaseGrabbedPlayer(PlayerInteract target)
    {
        if (target == null)
            return;

        if (!grabbedPlayers.Contains(target))
            return;

        grabbedPlayers.Remove(target);

        grabData.RemoveAll(data => data.target == target);

        target.RemoveGrabber(this);


        if (grabbedPlayers.Count == 0)
        {
            if (animator != null)
                animator.SetBool("isGrabbing", false);
        }


        if (grabSource != null && grabDropClip != null)
        {
            grabSource.pitch = Random.Range(0.9f, 1f);
            grabSource.PlayOneShot(grabDropClip);
        }

        StartCoroutine(GrabCooldown());
    }


    // Release all grabbing players
    public void ReleaseAllGrabbedPlayers()
    {
        List<PlayerInteract> playersToRelease =
            new List<PlayerInteract>(grabbedPlayers);

        foreach (PlayerInteract target in playersToRelease)
        {
            ReleaseGrabbedPlayer(target);
        }
    }

    //Escaped player releases from everyone
    public void EscapeFromAllGrabbers()
    {
        List<PlayerInteract> grabbers =
            new List<PlayerInteract>(grabbingPlayers);

        foreach (PlayerInteract grabber in grabbers)
        {
            if (grabber != null)
            {
                // Prevent this specific grabber from grabbing this
                // player again for the escape cooldown duration.
                grabber.StartEscapeCooldownFor(this);

                // Remove this player from the grabber's list.
                grabber.RemoveGrabbedPlayerWithoutCooldown(this);
            }
        }

        grabbingPlayers.Clear();

        StopBeingGrabbed();

        PlayerEscapeUI escapeUI =
            GetComponent<PlayerEscapeUI>();

        if (escapeUI != null)
            escapeUI.StopBeingGrabbed();
    }


    //Removes player without normal grab cooldown
    public void RemoveGrabbedPlayerWithoutCooldown(PlayerInteract target)
    {
        if (target == null)
            return;

        if (!grabbedPlayers.Contains(target))
            return;

        grabbedPlayers.Remove(target);

        grabData.RemoveAll(data => data.target == target);


        if (grabbedPlayers.Count == 0)
        {
            if (animator != null)
                animator.SetBool("isGrabbing", false);
        }


        if (grabSource != null && grabDropClip != null)
        {
            grabSource.pitch = Random.Range(0.9f, 1f);
            grabSource.PlayOneShot(grabDropClip);
        }
    }

    public void StartEscapeCooldownFor(PlayerInteract target)
    {
        if (target == null)
            return;

        if (escapeCooldowns.ContainsKey(target))
        {
            StopCoroutine(escapeCooldowns[target]);
            escapeCooldowns.Remove(target);
        }

        Coroutine cooldown =
            StartCoroutine(EscapeCooldownRoutine(target));

        escapeCooldowns.Add(target, cooldown);
    }

    private IEnumerator EscapeCooldownRoutine(PlayerInteract target)
    {
        yield return new WaitForSeconds(escapeCooldownTime);

        if (escapeCooldowns.ContainsKey(target))
            escapeCooldowns.Remove(target);
    }

    public IEnumerator GrabCooldown()
    {
        canGrab = false;

        yield return new WaitForSeconds(grabCooldownTime);

        canGrab = true;
    }


    public void ForceRelease()
    {
        ReleaseAllGrabbedPlayers();

        EscapeFromAllGrabbers();
    }


    public void ForceReleaseAll()
    {
        ForceRelease();
    }


    public void ResetGrabState()
    {
        ReleaseAllGrabbedPlayers();

        EscapeFromAllGrabbers();

        canGrab = true;
        nearbyPlayer = null;
    }

    private void FaceGrabbedPlayer()
    {
        PlayerInteract interact =
            GetComponent<PlayerInteract>();

        if (interact == null ||
            !interact.IsGrabbingSomeone)
            return;


        Vector3 direction =
            interact.GetGrabbedPlayerPosition() -
            transform.position;


        direction.y = 0f;


        if (direction.sqrMagnitude < 0.01f)
            return;


        Quaternion targetRotation =
            Quaternion.LookRotation(direction);


        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                10f * Time.deltaTime);
    }

    public Vector3 GetGrabbedPlayerPosition()
    {
        if (grabbedPlayers.Count == 0)
            return transform.position;

        return grabbedPlayers[0].transform.position;
    }


    // ===================================== END OF PLAYER GRAB ============================================

    // ======================================== ITEM PICKUP ================================================

    private void HandlePickup()
    {
        if (heldPickup != null || cherryPickupCooldown > 0f) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius);
        GameObject closestPickup = null;
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            LevelPickup lp = hit.GetComponent<LevelPickup>() ?? hit.GetComponentInParent<LevelPickup>();
            if (lp == null || lp.isHeld) continue;
            float dist = Vector3.Distance(transform.position, hit.ClosestPoint(transform.position));
            if (dist < closestDist) { closestDist = dist; closestPickup = lp.gameObject; }
        }

        if (closestPickup == null) return;

        heldPickup = closestPickup;

        LevelPickup pickup = heldPickup.GetComponent<LevelPickup>();
        if (pickup != null)
        {
            pickup.isHeld = true;
            pickup.playerHolding = gameObject;
        }

        if (pickupSource != null && pickupClip != null)
        {
            pickupSource.pitch = Random.Range(0.95f, 1.05f);
            pickupSource.PlayOneShot(pickupClip);
        }

        Rigidbody rbCherry = heldPickup.GetComponent<Rigidbody>();
        if (rbCherry != null) rbCherry.isKinematic = true;

        heldPickup.transform.SetParent(handHoldPoint);
        SetCherryCollision(false);
        heldPickup.transform.localPosition = Vector3.zero;


        if (pickup != null)
        {
            if (pickup.useProjectileThrow)
            {
                // Cherry
                projectileScript?.PickUpCherry(heldPickup);
            }
            else
            {
                // Snowball
                snowballThrow?.PickUpSnowball(heldPickup);
            }
        }

        if (animator != null)
            StartCoroutine(PlayPickupAnimation());
    }

    public void CancelAimAndDrop()
    {
        if (heldPickup == null) return;

        if (pickupSource != null && cherryDropClip != null)
        {
            pickupSource.pitch = Random.Range(0.9f, 1.0f);
            pickupSource.PlayOneShot(cherryDropClip);
        }

        LevelPickup pickup = heldPickup.GetComponent<LevelPickup>();

        if (pickup != null)
        {
            if (pickup.useProjectileThrow)
                projectileScript?.CancelAim();
            else
                snowballThrow?.CancelAim();
        }

        Rigidbody rbCherry = heldPickup.GetComponent<Rigidbody>();
        heldPickup.transform.SetParent(null);
        if (rbCherry != null) rbCherry.isKinematic = false;

        SetCherryCollision(true);

        if (animator != null) animator.SetBool("isPickingUp", false);

        pickup = heldPickup.GetComponent<LevelPickup>();
        if (pickup != null)
        {
            pickup.isHeld = false;
            pickup.playerHolding = null;
        }

        heldPickup = null;
        cherryPickupCooldown = 0.5f;
    }

    public void ForceDrop()
    {
        CancelAimAndDrop();
        heldPickup = null;
    }

    public void NotifyThrowEnded()
    {
        heldPickup = null;
        cherryPickupCooldown = 0.5f;
    }

    private IEnumerator PlayPickupAnimation()
    {
        animator.SetBool("isPickingUp", true);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
    }

    // ===== TRIGGER ZONES =====

    private void OnTriggerEnter(Collider other)
    {
        SnowballPile pile = other.GetComponent<SnowballPile>();

        if (pile != null)
        {
            nearbySnowPile = pile;
        }

        Playermovement pm = other.GetComponent<Playermovement>() ?? other.GetComponentInParent<Playermovement>();
        if (pm == null || pm.playerIndex == player.playerIndex) return;

        if (nearbyPlayer == null)
        {
            nearbyPlayer = pm.gameObject;
        }
        else
        {
            float distNew = Vector3.Distance(transform.position, pm.transform.position);
            float distCurrent = Vector3.Distance(transform.position, nearbyPlayer.transform.position);
            if (distNew < distCurrent) nearbyPlayer = pm.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SnowballPile pile = other.GetComponent<SnowballPile>();

        if (pile == nearbySnowPile)
        {
            nearbySnowPile = null;
        }

        Playermovement pm = other.GetComponent<Playermovement>() ?? other.GetComponentInParent<Playermovement>();
        if (pm != null && pm.gameObject == nearbyPlayer) nearbyPlayer = null;
    }

    // ===== HELPERS =====

    private void GiveSnowballs()
    {
        snowballsRemaining = 3;

        Debug.Log("Player received 3 snowballs.");

        SpawnSnowballInHand(true);
    }

    public void OnSnowballThrown()
    {
        snowballsRemaining--;

        Debug.Log("Snowballs left: " + snowballsRemaining);

        if (snowballsRemaining > 0)
        {
            SpawnSnowballInHand(false);
        }
    }

    private void SpawnSnowballInHand(bool ignoreFirstRelease)
    {
        GameObject snowball = Instantiate(
            snowballPrefab,
            handHoldPoint.position,
            handHoldPoint.rotation);

        Snowball snowballScript = snowball.GetComponent<Snowball>();

        if (snowballScript != null)
        {
            snowballScript.SetOwner(gameObject);
        }

        heldPickup = snowball;

        LevelPickup pickup = heldPickup.GetComponent<LevelPickup>();

        if (pickup != null)
        {
            pickup.isHeld = true;
            pickup.playerHolding = gameObject;
        }

        Rigidbody rb = heldPickup.GetComponent<Rigidbody>();

        if (rb != null)
            rb.isKinematic = true;

        heldPickup.transform.SetParent(handHoldPoint);
        heldPickup.transform.localPosition = Vector3.zero;

        SetCherryCollision(false);

        snowballThrow?.PickUpSnowball(heldPickup);

        if (animator != null)
            StartCoroutine(PlayPickupAnimation());
    }

    private void SetCherryCollision(bool enabled)
    {
        if (heldPickup == null) return;
        Collider[] playerCols = GetComponentsInChildren<Collider>();
        Collider[] cherryCols = heldPickup.GetComponentsInChildren<Collider>();
        foreach (var p in playerCols)
            foreach (var c in cherryCols)
                Physics.IgnoreCollision(p, c, !enabled);
    }
}
