using Assets.DuckType.Jiggle;
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


    // ===== PRIVATE =====
    private Playermovement player;
    private Projectile projectileScript;
    private SnowballThrow snowballThrow;
    private Animator animator;
    private Jiggle[] jiggleParts;
    private PlayerPowerupHandler powerupHandler;
    private Collider myCollider;

    private bool canGrab = true;

    public bool IsBeingGrabbed => grabbingPlayers.Count > 0;

    public int NumberOfGrabbers => grabbingPlayers.Count;

    private GameObject nearbyPlayer;
    private bool ltWasHeld = false;

    // SNOWBALL STUFF
    private SnowballPile nearbySnowPile;
    [SerializeField] private GameObject snowballPrefab;

    private int snowballsRemaining = 0;


    private bool rtHeld = false;
    private float rtHoldTime = 0f;
    private bool rtWasDown = false;
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
        jiggleParts = GetComponentsInChildren<Jiggle>();
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
            if (heldPickup == null)
            {
                OnInteractPressed();
            }
        }

        if (rtReleased)
        {
            ReleaseAllGrabbedPlayers();
        }

        rtWasDown = rt;
    }

    private void FixedUpdate()
    {
        UpdateGrabbedPlayers();
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

    // ===== GRAB =====

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


        // =====================================================
        // ADD THIS PLAYER AS A GRABBER
        // =====================================================

        if (grabbedPlayers.Contains(targetInteract))
            return false;


        grabbedPlayers.Add(targetInteract);

        targetInteract.AddGrabber(this);

        // =====================================================
        // ANIMATION
        // =====================================================

        if (animator != null)
            animator.SetBool("isGrabbing", true);


        // =====================================================
        // AUDIO
        // =====================================================

        if (grabSource != null && grabClip != null)
        {
            grabSource.pitch = Random.Range(0.95f, 1.05f);
            grabSource.PlayOneShot(grabClip);
        }

        return true;
    }

    private void UpdateGrabbedPlayers()
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


            // =====================================================
            // GET THE CONNECTION DIRECTION
            // =====================================================

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


            // =====================================================
            // DETERMINE HOW FAR FROM THE IDEAL DISTANCE
            // THEY ARE
            // =====================================================

            float distanceError =
                distance -
                grabConnectionDistance;


            // =====================================================
            // GET BOTH PLAYERS' HORIZONTAL VELOCITIES
            // =====================================================

            Vector3 grabberVelocity =
                grabberRigidbody.linearVelocity;

            Vector3 grabbedVelocity =
                grabbedRigidbody.linearVelocity;

            grabberVelocity.y = 0f;
            grabbedVelocity.y = 0f;


            // =====================================================
            // RELATIVE VELOCITY
            // =====================================================

            Vector3 relativeVelocity =
                grabbedVelocity -
                grabberVelocity;


            // How quickly they are moving away from each other
            float separatingVelocity =
                Vector3.Dot(
                    relativeVelocity,
                    direction);


            // =====================================================
            // SPRING FORCE
            // =====================================================

            float springForce =
                distanceError *
                grabConnectionStrength;


            // =====================================================
            // DAMPING
            // =====================================================

            float dampingForce =
                separatingVelocity *
                grabConnectionDamping;


            float totalForce =
                springForce +
                dampingForce;


            // =====================================================
            // APPLY FORCE TO BOTH PLAYERS
            // =====================================================

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
    }

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


    // =========================================================
    // CALLED ON THE PLAYER BEING GRABBED
    // =========================================================

    public void AddGrabber(PlayerInteract grabber)
    {
        if (grabber == null)
            return;

        if (grabbingPlayers.Contains(grabber))
            return;

        grabbingPlayers.Add(grabber);


        // =====================================================
        // APPLY SLOWDOWN
        // =====================================================

        if (grabbingPlayers.Count == 1)
        {
            StartCoroutine(ApplyGrabSlowdown());


            // Start escape UI
            PlayerEscapeUI escapeUI =
                GetComponent<PlayerEscapeUI>();

            if (escapeUI != null)
                escapeUI.StartBeingGrabbed();
        }


        // =====================================================
        // ANIMATION
        // =====================================================

        if (animator != null)
            animator.SetBool("isGrabbed", true);
    }


    public void RemoveGrabber(PlayerInteract grabber)
    {
        if (grabber == null)
            return;

        grabbingPlayers.Remove(grabber);


        // =====================================================
        // IF NO ONE IS GRABBING ANYMORE
        // =====================================================

        if (grabbingPlayers.Count == 0)
        {
            StopBeingGrabbed();

            PlayerEscapeUI escapeUI =
                GetComponent<PlayerEscapeUI>();

            if (escapeUI != null)
                escapeUI.StopBeingGrabbed();
        }
    }


    // =========================================================
    // SLOWDOWN
    // =========================================================

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
            animator.SetBool("isGrabbed", false);
    }

    public void ReleaseGrabbedPlayer(PlayerInteract target)
    {
        if (target == null)
            return;

        if (!grabbedPlayers.Contains(target))
            return;

        grabbedPlayers.Remove(target);

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

    // =========================================================
    // RELEASE EVERYONE THIS PLAYER IS GRABBING
    // =========================================================

    public void ReleaseAllGrabbedPlayers()
    {
        List<PlayerInteract> playersToRelease =
            new List<PlayerInteract>(grabbedPlayers);

        foreach (PlayerInteract target in playersToRelease)
        {
            ReleaseGrabbedPlayer(target);
        }
    }


    // =========================================================
    // ESCAPED PLAYER RELEASES FROM EVERYONE
    // =========================================================

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


    // =========================================================
    // REMOVE A PLAYER WITHOUT NORMAL GRAB COOLDOWN
    // =========================================================

    public void RemoveGrabbedPlayerWithoutCooldown(PlayerInteract target)
    {
        if (target == null)
            return;

        if (!grabbedPlayers.Contains(target))
            return;

        grabbedPlayers.Remove(target);


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

    // ===== CHERRY =====

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

        SetJiggle(false);

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
        SetJiggle(true);

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

    private void SetJiggle(bool enabled)
    {
        if (jiggleParts == null) return;
        foreach (var jiggle in jiggleParts)
            if (jiggle != null) jiggle.enabled = enabled;
    }
}
