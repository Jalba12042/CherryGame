using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Projectile : MonoBehaviour
{
    [Header("References")]
    public Transform launchPoint;

    [Header("Throw Settings")]
    public float launchSpeed = 15f;

    [Header("Throw Timing")]
    public float throwDelay = 0.18f;

    [Header("Arc Tuning")]
    [Tooltip("How much upward velocity to add for the arc shape.")]
    public float arcHeight = 1.5f;

    [Range(0.05f, 2f)]
    [Tooltip("Scales how far the throw goes.")]
    public float distanceMultiplier = 0.33f;

    [Tooltip("Optional clamp to prevent extreme forward speed.")]
    public float maxForwardSpeed = 15f;

    [Header("Air Speed Tuning")]
    [Tooltip("Higher = faster air time without changing landing position.")]
    public float airSpeedMultiplier = 1f;

    [Header("Trajectory Visual")]
    //public LineRenderer lineRenderer;
    public GameObject landingMarkerPrefab;

    private GameObject landingMarkerInstance;

    [Header("Trajectory Settings")]
    public int linePoints = 50;
    public float timeStep = 0.1f;

    [SerializeField] private LayerMask groundLayer;

    [Header("Throw Cooldown")]
    [SerializeField] private float throwCooldown = 4f;

    private float throwCooldownTimer = 0f;
    private bool canThrow = true;

    [Header("Cooldown UI")]
    public GameObject cooldownBarObject;
    public UnityEngine.UI.Image cooldownFillImage;

    private Playermovement owner;
    private GameObject heldCherry;
    private bool isHoldingCherry = false;
    private bool isAiming = false;
    private bool pendingThrow = false;

    private float currentPower = 0f;

    public bool IsAiming() => isAiming;
    public bool IsThrowPending() => pendingThrow;

    //private TrailRenderer cherryTrail;
    public bool canThrowEnabled = true;

    void Start()
    {
        //if (lineRenderer != null)
        //lineRenderer.positionCount = 0;

        /*if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }*/

        if (landingMarkerPrefab != null)
        {
            landingMarkerInstance = Instantiate(landingMarkerPrefab);
            landingMarkerInstance.SetActive(false);
        }
    }

    void Update()
    {
        if (!canThrowEnabled)
        {
            // disable ONLY throwing, NOT everything
            isAiming = false;
            return;
        }

        if (!canThrow)
        {
            throwCooldownTimer -= Time.deltaTime;
            throwCooldownTimer = Mathf.Max(0, throwCooldownTimer);

            if (cooldownFillImage != null)
            {
                cooldownFillImage.fillAmount = throwCooldownTimer / throwCooldown;
            }

            if (throwCooldownTimer <= 0)
            {
                canThrow = true;

                if (cooldownBarObject != null)
                    cooldownBarObject.SetActive(false);
            }
        }

        if (owner == null) return;

        var gamepad = owner.GetAssignedGamepad();
        if (gamepad == null) return;

        launchPoint.forward = owner.transform.forward;


        float ltValue = gamepad.leftTrigger.ReadValue();
        //float rtValue = gamepad.rightTrigger.ReadValue();

        if (!isHoldingCherry)
        {
            isAiming = false;
            currentPower = 0f;

            if (landingMarkerInstance != null)
                landingMarkerInstance.SetActive(false);

            return;
        }

        // HOLD LT
        if (ltValue > 0.2f)
        {
            isAiming = true;
            owner.animator.SetBool("isAiming", true);

            currentPower = Mathf.Lerp(currentPower, ltValue, Time.deltaTime * 8f);

            //if (lineRenderer != null)
                //lineRenderer.enabled = true;

            DrawTrajectory(currentPower);
        }
        // RELEASE LT
        else if (isAiming && ltValue <= 0.2f && canThrow)
        {
            //if (lineRenderer != null)
                //lineRenderer.enabled = false;

            if (landingMarkerInstance != null)
                landingMarkerInstance.SetActive(false);

            float finalPower = Mathf.Max(currentPower, 0.25f);

            pendingThrow = true;
            //owner.GetComponent<PlayerCherry>()?.NotifyThrowStarted();

            // START COOLDOWN
            canThrow = false;
            throwCooldownTimer = throwCooldown;

            if (cooldownBarObject != null)
                cooldownBarObject.SetActive(true);

            owner.animator.SetTrigger("doThrow");
            owner.animator.SetBool("isAiming", false);
            owner.animator.SetBool("isPickingUp", false);

            owner.StartCoroutine(DelayedThrow(finalPower));

            isAiming = false;
            currentPower = 0f;
        }
    }

    public void PickUpCherry(GameObject cherryObject)
    {
        heldCherry = cherryObject;
        isHoldingCherry = true;
        Cherry cherryScript = cherryObject.GetComponent<Cherry>();

        if (cherryScript != null)
        {
            cherryScript.isHeld = true;
            cherryScript.playerHolding = gameObject;
            cherryScript.DisableTrail();
        }

        /*cherryTrail = heldCherry.GetComponent<TrailRenderer>();
        if (cherryTrail != null)
            cherryTrail.emitting = false;*/

        Rigidbody rb = heldCherry.GetComponent<Rigidbody>();
        if (rb != null)
        {
            //rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        heldCherry.transform.SetParent(launchPoint);
        heldCherry.transform.localPosition = Vector3.zero;
    }

    void DrawTrajectory(float power)
    {
        if (heldCherry == null) return;

        Vector3 origin = launchPoint.position;

        float baseSpeed = launchSpeed * power;

        float upSpeed = baseSpeed * arcHeight;
        float forwardSpeed = baseSpeed * distanceMultiplier / (1f + arcHeight);

        if (maxForwardSpeed > 0f)
            forwardSpeed = Mathf.Min(forwardSpeed, maxForwardSpeed);

        Vector3 velocity =
            launchPoint.forward * forwardSpeed +
            Vector3.up * upSpeed;

        // Apply air speed scaling exactly like ThrowCherry
        Vector3 scaledVelocity = velocity * airSpeedMultiplier;
        Vector3 gravity = Physics.gravity * airSpeedMultiplier;

        //lineRenderer.positionCount = linePoints;

        Vector3 previousPoint = origin;

        for (int i = 0; i < linePoints; i++)
        {
            float t = i * timeStep;

            Vector3 point =
                origin +
                scaledVelocity * t +
                0.5f * gravity * t * t;

            //lineRenderer.SetPosition(i, point);

            if (i > 0)
            {
                Vector3 direction = point - previousPoint;
                float distance = direction.magnitude;

                if (Physics.Raycast(previousPoint, direction.normalized,
                    out RaycastHit hit, distance, groundLayer))
                {
                    //lineRenderer.positionCount = i + 1;
                    //lineRenderer.SetPosition(i, hit.point);

                    if (landingMarkerInstance != null)
                    {
                        landingMarkerInstance.transform.position = hit.point;
                        landingMarkerInstance.SetActive(true);
                    }

                    return;
                }
            }

            previousPoint = point;
        }

        if (landingMarkerInstance != null)
            landingMarkerInstance.SetActive(false);
    }

    void ThrowCherry(float finalPower)
    {
        if (heldCherry == null) return;

        Rigidbody rb = heldCherry.GetComponent<Rigidbody>();
        heldCherry.transform.SetParent(null);

        Cherry cherryScript = heldCherry.GetComponent<Cherry>();
        if (cherryScript != null)
        {
            cherryScript.isHeld = false;
            cherryScript.playerHolding = null;
            cherryScript.EnableTrail();
        }

        /*cherryTrail = heldCherry.GetComponent<TrailRenderer>();

        if (cherryTrail != null)
        {
            cherryTrail.Clear();      // removes any old trail
            cherryTrail.emitting = true;  
        }*/


        if (rb != null)
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Ignore collision with player
            Collider cherryCol = heldCherry.GetComponent<Collider>();
            Collider[] playerCols = owner.GetComponentsInChildren<Collider>();

            foreach (var col in playerCols)
                Physics.IgnoreCollision(cherryCol, col, true);

            owner.StartCoroutine(ReenableCherryCollision(cherryCol, playerCols));

            // --- ARC CALCULATION ---
            float baseSpeed = launchSpeed * finalPower;

            float upSpeed = baseSpeed * arcHeight;
            float forwardSpeed = baseSpeed * distanceMultiplier / (1f + arcHeight);

            if (maxForwardSpeed > 0f)
                forwardSpeed = Mathf.Min(forwardSpeed, maxForwardSpeed);

            Vector3 velocity = (launchPoint.forward * forwardSpeed + Vector3.up * upSpeed);

            // --- AIR SPEED CONTROL ---
            rb.linearVelocity = velocity * airSpeedMultiplier;

            rb.useGravity = false;
            owner.StartCoroutine(ApplyCustomGravity(rb));
        }

        heldCherry = null;
    }

    private IEnumerator ApplyCustomGravity(Rigidbody rb)
    {
        while (rb != null && !rb.IsSleeping())
        {
            rb.AddForce(Physics.gravity * airSpeedMultiplier, ForceMode.Acceleration);
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator ReenableCherryCollision(Collider cherryCol, Collider[] playerCols)
    {
        yield return new WaitForSeconds(0.3f);

        foreach (var col in playerCols)
            Physics.IgnoreCollision(cherryCol, col, false);
    }

    private IEnumerator DelayedThrow(float power)
    {
        yield return new WaitForSeconds(throwDelay);

        if (heldCherry != null)
            ThrowCherry(power);

        pendingThrow = false;
        owner.GetComponent<PlayerCherry>()?.NotifyThrowEnded();
    }

    public void SetOwner(Playermovement player)
    {
        owner = player;
    }

    public void CancelAim()
    {
        isAiming = false;
        isHoldingCherry = false;
        heldCherry = null;

        owner.animator.SetBool("isAiming", false);
        owner.animator.SetBool("isPickingUp", false);

        currentPower = 0f;

        if (landingMarkerInstance != null)
            landingMarkerInstance.SetActive(false);
    }

    public void DisableThrowing()
    {
        canThrowEnabled = false;

        // Only stop aiming — DO NOT touch pickup animation
        isAiming = false;

        if (owner != null)
            owner.animator.SetBool("isAiming", false);

        if (landingMarkerInstance != null)
            landingMarkerInstance.SetActive(false);
    }

    public void EnableThrowing()
    {
        canThrowEnabled = true;
    }

    public void ForceResetCherry()
    {
        if (heldCherry != null)
        {
            // Unparent
            heldCherry.transform.SetParent(null);

            // Physics back on
            Rigidbody rb = heldCherry.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = false;

            // Reset cherry script
            Cherry cherryScript = heldCherry.GetComponent<Cherry>();
            if (cherryScript != null)
            {
                cherryScript.isHeld = false;
                cherryScript.playerHolding = null;
            }
        }

        // FULL RESET
        heldCherry = null;
        isHoldingCherry = false;
        isAiming = false;
        pendingThrow = false;
        currentPower = 0f;

        if (owner != null)
        {
            owner.animator.SetBool("isAiming", false);
            owner.animator.SetBool("isPickingUp", false);
        }

        if (landingMarkerInstance != null)
        {
            landingMarkerInstance.SetActive(false);
        }
    }

}