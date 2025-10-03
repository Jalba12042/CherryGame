using UnityEngine;
using UnityEngine.InputSystem;

public class Projectile : MonoBehaviour
{
    [Header("References")]
    public Transform launchPoint;
    public GameObject cherry;
    public LineRenderer lineRenderer;
    public GameObject landingMarker;

    [Header("Throw Settings")]
    public float launchSpeed = 10f;      // max throw speed
    public int linePoints = 50;
    public float timeStep = 0.1f;

    // Internal state
    private bool isHoldingCherry = false;
    private bool isAiming = false;
    private float throwPower = 0f;       // stores the LT value at release
    private Gamepad assignedGamepad;
    private GameObject heldCherry;

    void Start()
    {
        if (Gamepad.all.Count > 0)
            assignedGamepad = Gamepad.all[0]; // assign first controller by default

        if (lineRenderer != null)
            lineRenderer.positionCount = 0;

        if (landingMarker != null)
            landingMarker.SetActive(false);
    }

    void Update()
    {
        if (assignedGamepad == null) return;

        float ltValue = assignedGamepad.leftTrigger.ReadValue();

        // While holding a cherry and LT pressed
        if (isHoldingCherry && ltValue > 0.1f)
        {
            isAiming = true;
            lineRenderer.enabled = true;

            // Enable landing marker when aiming
            if (landingMarker != null)
                landingMarker.SetActive(true);

            throwPower = ltValue; // store current trigger value

            DrawTrajectory(throwPower);
        }
        // LT released
        else if (isAiming && ltValue <= 0.1f)
        {
            ThrowCherry();

            isAiming = false;
            lineRenderer.enabled = false;
            isHoldingCherry = false;
            heldCherry = null;

            if (landingMarker != null)
                landingMarker.SetActive(false);

            throwPower = 0f;
        }
        else
        {
            // Not aiming
            if (lineRenderer != null)
                lineRenderer.enabled = false;
        }
    }

    // Call externally when player picks up a cherry
    public void PickUpCherry(GameObject cherryObject)
    {
        heldCherry = cherryObject;
        isHoldingCherry = true;

        // Parent to launch point
        heldCherry.transform.SetParent(launchPoint);
        heldCherry.transform.localPosition = Vector3.zero;

        Rigidbody rb = heldCherry.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;

        // Don't enable landing marker here
        if (landingMarker != null)
            landingMarker.SetActive(false);
    }


    void DrawTrajectory(float power)
    {
        if (heldCherry == null) return;

        Vector3 origin = launchPoint.position;
        Vector3 velocity = launchPoint.forward * (launchSpeed * power);

        lineRenderer.positionCount = linePoints;
        for (int i = 0; i < linePoints; i++)
        {
            float t = i * timeStep;
            Vector3 point = origin + velocity * t + 0.5f * Physics.gravity * t * t;
            lineRenderer.SetPosition(i, point);
        }

        // Calculate predicted landing point
        float y0 = origin.y;
        float vy = velocity.y;
        float g = Physics.gravity.y;
        float discriminant = vy * vy - 2 * g * y0;
        float timeToLand = 0f;

        if (discriminant >= 0f)
            timeToLand = (-vy - Mathf.Sqrt(discriminant)) / g;

        Vector3 landingPosition = origin + velocity * timeToLand + 0.5f * Physics.gravity * timeToLand * timeToLand;

        if (landingMarker != null)
            landingMarker.transform.position = landingPosition;
    }

    void ThrowCherry()
    {
        if (heldCherry == null) return;

        Rigidbody rb = heldCherry.GetComponent<Rigidbody>();
        heldCherry.transform.SetParent(null);

        if (rb != null)
        {
            rb.isKinematic = false;

            // Fix 1: continuous collision detection
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Old style throw: forward + upward for a nice arc
            Vector3 throwDirection = launchPoint.forward + Vector3.up * 0.5f; // tweak 0.5f to control arc
            rb.linearVelocity = throwDirection.normalized * launchSpeed;
        }

        heldCherry = null;

        if (landingMarker != null)
            landingMarker.SetActive(false);
    }


}
