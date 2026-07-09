using UnityEngine;

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

    private Vector3 lastLeftPrint;
    private Vector3 lastRightPrint;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        lastLeftPrint = Vector3.one * 9999f;
        lastRightPrint = Vector3.one * 9999f;
    }

    void Update()
    {
        if (rb == null)
            return;

        // Don't make prints while standing still
        if (new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude < 0.5f)
            return;

        CheckFoot(leftFoot, ref lastLeftPrint);
        CheckFoot(rightFoot, ref lastRightPrint);
    }

    void CheckFoot(Transform foot, ref Vector3 lastPrint)
    {
        if (foot == null)
            return;

        RaycastHit hit;

        if (Physics.Raycast(
            foot.position + Vector3.up * 0.1f,
            Vector3.down,
            out hit,
            rayDistance,
            sandLayer))
        {
            if (Vector3.Distance(hit.point, lastPrint) >= footprintSpacing)
            {
                Instantiate(
                    footprintPrefab,
                    hit.point + Vector3.up * 0.01f,
                    Quaternion.Euler(90, Random.Range(0, 360), 0));

                lastPrint = hit.point;
            }
        }
    }
}