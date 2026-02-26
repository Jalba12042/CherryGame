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
    public LineRenderer lineRenderer;
    public GameObject landingMarkerPrefab;

    private GameObject landingMarkerInstance;

    [Header("Trajectory Settings")]
    public int linePoints = 50;
    public float timeStep = 0.1f;

    [SerializeField] private LayerMask groundLayer;

    private Playermovement owner;
    private GameObject heldCherry;
    private bool isHoldingCherry = false;
    private bool isAiming = false;
    private bool pendingThrow = false;

    private float currentPower = 0f;

    public bool IsAiming() => isAiming;
    public bool IsThrowPending() => pendingThrow;

    void Start()
    {
        if (lineRenderer != null)
            lineRenderer.positionCount = 0;

        if (landingMarkerPrefab != null)
        {
            landingMarkerInstance = Instantiate(landingMarkerPrefab);
            landingMarkerInstance.SetActive(false);
        }
    }

    void Update()
    {
        if (owner == null) return;

        var gamepad = owner.GetAssignedGamepad();
        if (gamepad == null) return;

        launchPoint.forward = owner.transform.forward;

        float ltValue = gamepad.leftTrigger.ReadValue();

        if (!isHoldingCherry)
        {
            isAiming = false;
            currentPower = 0f;
            return;
        }

        // HOLD LT
        if (ltValue > 0.1f)
        {
            isAiming = true;
            owner.animator.SetBool("isAiming", true);

            currentPower = Mathf.Lerp(currentPower, ltValue, Time.deltaTime * 8f);

            if (lineRenderer != null)
                lineRenderer.enabled = true;

            DrawTrajectory(currentPower);
        }
        // RELEASE LT
        else if (isAiming && ltValue <= 0.1f)
        {
            if (lineRenderer != null)
                lineRenderer.enabled = false;

            if (landingMarkerInstance != null)
                landingMarkerInstance.SetActive(false);

            float finalPower = Mathf.Max(currentPower, 0.25f);

            pendingThrow = true;
            owner.GetComponent<PlayerCherry>()?.NotifyThrowStarted();

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

        Rigidbody rb = heldCherry.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
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

        lineRenderer.positionCount = linePoints;

        Vector3 previousPoint = origin;

        for (int i = 0; i < linePoints; i++)
        {
            float t = i * timeStep;

            Vector3 point =
                origin +
                scaledVelocity * t +
                0.5f * gravity * t * t;

            lineRenderer.SetPosition(i, point);

            if (i > 0)
            {
                Vector3 direction = point - previousPoint;
                float distance = direction.magnitude;

                if (Physics.Raycast(previousPoint, direction.normalized,
                    out RaycastHit hit, distance, groundLayer))
                {
                    lineRenderer.positionCount = i + 1;
                    lineRenderer.SetPosition(i, hit.point);

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
    }
}



























/*{
    [Header("References")]
    public Transform launchPoint;
    public GameObject cherry;
    public LineRenderer lineRenderer;
    public GameObject landingMarkerPrefab;
    private GameObject landingMarkerInstance;

    [Header("Throw Settings")]
    public float launchSpeed = 15f;      // max throw speed
    public int linePoints = 50;
    public float timeStep = 0.1f;

    [Header("Throw Timing")]
    public float throwDelay = 0.18f;


    // Internal state
    private bool isHoldingCherry = false;
    private bool isAiming = false;
    private float throwPower = 0f;       // stores the LT value at release
    private float currentPower = 0f;  // tracks LT power while aiming
    private Gamepad assignedGamepad;
    private GameObject heldCherry;
    private bool pendingThrow = false;




    [SerializeField] private LayerMask groundLayer;

    private Playermovement owner;


    [Header("Arc Tuning")]
    [Tooltip("How much upward velocity to add for the arc shape (bigger = taller).")]
    public float arcHeight = 1.5f;

    [Tooltip("Scales how far the trajectory goes (smaller = shorter distance).")]
    [Range(0.05f, 2f)]
    public float distanceMultiplier = 0.33f;

    [Tooltip("Optional extra clamp on forward speed to prevent flying off the map.")]
    public float maxForwardSpeed = 15f;

    [Tooltip("If you increase this it will increase the distance of the aim arc")]
    public float travelSpeedMultiplier = 1.5f;

    [Header("Air Speed Tuning")]
    [Tooltip("Higher = faster time in air without changing arc shape.")]
    public float airSpeedMultiplier = 1f;

    public bool IsAiming() => isAiming;
    public bool IsThrowPending() => pendingThrow;

    void Start()
    {
       
        if (Gamepad.all.Count > 0)
            assignedGamepad = Gamepad.all[0]; // assign first controller by default
        

        if (lineRenderer != null)
            lineRenderer.positionCount = 0;

        if (landingMarkerPrefab != null)
        {
            landingMarkerInstance = Instantiate(landingMarkerPrefab);
            landingMarkerInstance.SetActive(false);
        }
    }

    void Update()
    {
        if (owner == null) return;

        var gamepad = owner.GetAssignedGamepad();
        if (gamepad == null) return;
        

        // Always aim wherever the player is facing
        launchPoint.forward = owner.transform.forward;

        float ltValue = gamepad.leftTrigger.ReadValue();

        // Stop aiming if you no longer hold a cherry
        if (!isHoldingCherry)
        {
            isAiming = false;
            if (lineRenderer != null) lineRenderer.enabled = false;
            if (landingMarkerInstance != null) landingMarkerInstance.SetActive(false);
            throwPower = 0f;
            return; // early exit
        }

        if (isHoldingCherry && ltValue > 0.1f)
        {                                                  
            isAiming = true; 
            lineRenderer.enabled = true;

            owner.animator.SetBool("isAiming", true);

            if (landingMarkerInstance != null)
                landingMarkerInstance.SetActive(true);

            // continuously store LT power (never instantly reset)
            currentPower = Mathf.Lerp(currentPower, ltValue, Time.deltaTime * 8f);


            /*Vector2 aimInput = gamepad.rightStick.ReadValue();

            if (aimInput.sqrMagnitude > 0.1f)
            {
                Transform cam = Camera.main.transform;

                Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
                Vector3 camRight = Vector3.Scale(cam.right, new Vector3(1, 0, 1)).normalized;

                Vector3 aimDir = (camForward * aimInput.y + camRight * aimInput.x).normalized;

                //Only rotates the throw direction — player stays still
                launchPoint.forward = aimDir;
            }*/

/*DrawTrajectory(currentPower);
            return;
        }
        else if (isAiming && ltValue <= 0.1f)
        {
            float finalPower = Mathf.Max(currentPower, 0.25f); // ensure some minimum throw

            // mark that a throw is pending so PlayerCherry won't drop it
            pendingThrow = true;

            // notify the PlayerCherry on the owner (optional redundancy)
            owner.GetComponent<PlayerCherry>()?.NotifyThrowStarted();

            // start the delayed throw coroutine
            owner.StartCoroutine(DelayedThrow(finalPower));

            // trigger animation
            owner.animator.SetTrigger("doThrow");

            // stop aiming visuals immediately (you can keep isAiming true if you prefer)
            owner.animator.SetBool("isAiming", false);
            owner.animator.SetBool("isPickingUp", false);

            // do not set isHoldingCherry = false here — let ThrowCherry handle it
            isAiming = false;
            lineRenderer.enabled = false;

            if (landingMarkerInstance != null)
                landingMarkerInstance.SetActive(false);

            // fully reset current power (we've saved finalPower above)
            currentPower = 0f;
        }

        /*else if (isAiming && ltValue <= 0.1f)
        {
            float finalPower = Mathf.Max(currentPower, 0.25f); // ensure some minimum throw
            StartCoroutine(DelayedThrow(finalPower));


            owner.animator.SetTrigger("doThrow");
            owner.animator.SetBool("isAiming", false);
            owner.animator.SetBool("isPickingUp", false);

            isAiming = false;
            lineRenderer.enabled = false;
            isHoldingCherry = false;
            heldCherry = null;

            if (landingMarkerInstance != null)
                landingMarkerInstance.SetActive(false);

            // fully reset after throw
            currentPower = 0f;
        }*/
//*}

/*
    public void PickUpCherry(GameObject cherryObject)
    {
        heldCherry = cherryObject;
        isHoldingCherry = true;

        // Parent to launch point (optional for visual purposes)
        heldCherry.transform.SetParent(launchPoint);
        heldCherry.transform.localPosition = Vector3.zero;

        if (landingMarkerInstance != null)
            landingMarkerInstance.SetActive(false);
    }




    void DrawTrajectory(float power)
    {
        if (heldCherry == null) return;

        Vector3 origin = launchPoint.position;

        // === Arc Tuning ===
        float baseSpeed = launchSpeed * power;

        // vertical speed for height
        float upSpeed = baseSpeed * arcHeight;

        // forward speed for distance (divided by arcHeight so high arcs go shorter)
        float forwardSpeed = baseSpeed * distanceMultiplier / (1f + arcHeight);

        // safety clamp so it never flies off the map
        if (maxForwardSpeed > 0f)
            forwardSpeed = Mathf.Min(forwardSpeed, maxForwardSpeed);

        // combine into full velocity
        //Vector3 velocity = launchPoint.forward * forwardSpeed + Vector3.up * upSpeed;

        Vector3 velocity = (launchPoint.forward * forwardSpeed + Vector3.up * upSpeed) * travelSpeedMultiplier;

        // === Draw the line ===
        lineRenderer.positionCount = linePoints;
        Vector3 previousPoint = origin;

        for (int i = 0; i < linePoints; i++)
        {
            //float t = i * timeStep;
            //Vector3 point = origin + velocity * t + 0.5f * Physics.gravity * t * t;

            float t = (i * timeStep) / airSpeedMultiplier;

            Vector3 gravity = Physics.gravity * airSpeedMultiplier;
            Vector3 scaledVelocity = velocity * airSpeedMultiplier;

            Vector3 point = origin + scaledVelocity * t + 0.5f * gravity * t * t;

            lineRenderer.SetPosition(i, point);

            if (i > 0)
            {
                Vector3 direction = point - previousPoint;
                float distance = direction.magnitude;

                // visualize the trajectory
                Debug.DrawLine(previousPoint, point, Color.red, 0.1f);

                // ground hit detection
                if (Physics.Raycast(previousPoint, direction.normalized, out RaycastHit hit, distance, groundLayer))
                {
                    if (landingMarkerInstance != null)
                    {
                        landingMarkerInstance.transform.position = hit.point;
                        landingMarkerInstance.SetActive(true);
                    }

                    lineRenderer.positionCount = i + 1;
                    lineRenderer.SetPosition(i, hit.point);
                    return;
                }
            }

            previousPoint = point;
        }

        // hide marker if no hit
        if (landingMarkerInstance != null)
            landingMarkerInstance.SetActive(false);
    }

    void ThrowCherry(float finalPower)
    {
        if (heldCherry == null) return;

        Rigidbody rb = heldCherry.GetComponent<Rigidbody>();
        heldCherry.transform.SetParent(null);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            //ignore collision with the player for 0.3s
            Collider cherryCol = heldCherry.GetComponent<Collider>();
            Collider[] playerCols = owner.GetComponentsInChildren<Collider>();

            foreach (var col in playerCols)
                Physics.IgnoreCollision(cherryCol, col, true);

            // 0.3s later re-enable collisions
            owner.StartCoroutine(ReenableCherryCollision(cherryCol, playerCols));

            //APPLY THROW FORCE
            float baseSpeed = launchSpeed * finalPower;
            float upSpeed = baseSpeed * arcHeight;
            float forwardSpeed = baseSpeed * distanceMultiplier / (1f + arcHeight);

            if (maxForwardSpeed > 0f)
                forwardSpeed = Mathf.Min(forwardSpeed, maxForwardSpeed);

            //Vector3 velocity = launchPoint.forward * forwardSpeed + Vector3.up * upSpeed;

            Vector3 velocity = (launchPoint.forward * forwardSpeed + Vector3.up * upSpeed) * travelSpeedMultiplier;

            //rb.linearVelocity = velocity;
            //Vector3 gravity = Physics.gravity * airSpeedMultiplier;
            rb.linearVelocity = velocity * airSpeedMultiplier;
            rb.useGravity = false;
            owner.StartCoroutine(ApplyCustomGravity(rb));
        }

        heldCherry = null;

        if (landingMarkerInstance != null)
            landingMarkerInstance.SetActive(false);
    }

    private IEnumerator ApplyCustomGravity(Rigidbody rb)
    {
        while (rb != null && !rb.IsSleeping())
        {
            rb.AddForce(Physics.gravity * airSpeedMultiplier, ForceMode.Acceleration);
            yield return null;
        }
    }

    private System.Collections.IEnumerator ReenableCherryCollision(Collider cherryCol, Collider[] playerCols)
    {
        yield return new WaitForSeconds(0.3f);

        foreach (var col in playerCols)
            Physics.IgnoreCollision(cherryCol, col, false);
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


        if (lineRenderer != null)
            lineRenderer.enabled = false;

        if (landingMarkerInstance != null)
            landingMarkerInstance.SetActive(false);

        throwPower = 0f;
    }

    private IEnumerator DelayedThrow(float power)
    {
        yield return new WaitForSeconds(throwDelay);

        // if the cherry still exists, actually throw
        if (heldCherry != null)
        {
            ThrowCherry(power);
        }

        // clear pending flag and notify player script
        pendingThrow = false;
        owner.GetComponent<PlayerCherry>()?.NotifyThrowEnded();

    }





}
*/