using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerFootprints : MonoBehaviour
{
    [Header("Feet")]
    [SerializeField] private Transform leftFoot;
    [SerializeField] private Transform rightFoot;

    [Header("Footprint")]
    [SerializeField] private GameObject footprintPrefab;
    [SerializeField] private LayerMask sandLayer;

    [SerializeField] private float rayDistance = 0.4f;
    [SerializeField] private float footprintSpacing = 0.25f;
    [SerializeField] private float minMoveSpeed = 0.5f;
    [SerializeField] private float footprintLifetime = 1f;

    private Vector3 lastLeftPrint;
    private Vector3 lastRightPrint;

    private Rigidbody rb;
    private bool footprintsEnabled;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        lastLeftPrint = Vector3.one * 9999f;
        lastRightPrint = Vector3.one * 9999f;

        // Only enable footprints on the BeachTest scene
        footprintsEnabled = SceneManager.GetActiveScene().name == "BeachTest";
    }

    void Update()
    {
        if (!footprintsEnabled || rb == null)
            return;

        // Only make footprints while moving
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (horizontalVelocity.magnitude < minMoveSpeed)
            return;

        CheckFoot(leftFoot, ref lastLeftPrint);
        CheckFoot(rightFoot, ref lastRightPrint);
    }

    void CheckFoot(Transform foot, ref Vector3 lastPrint)
    {
        if (foot == null)
            return;

        if (Physics.Raycast(
            foot.position + Vector3.up * 0.1f,
            Vector3.down,
            out RaycastHit hit,
            rayDistance,
            sandLayer))
        {
            if (Vector3.Distance(hit.point, lastPrint) >= footprintSpacing)
            {
                GameObject footprint = Instantiate(
                    footprintPrefab,
                    hit.point + Vector3.up * 0.01f,
                    Quaternion.Euler(90f, Random.Range(0f, 360f), 0f));

                // Remove the footprint after a short time
                Destroy(footprint, footprintLifetime);

                lastPrint = hit.point;
            }
        }
    }
}