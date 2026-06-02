using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ocean : MonoBehaviour
{
    [Header("Positions")]
    public Transform startPoint;
    public Transform inPoint;

    [Header("Timing")]
    public float rollInDuration = 3f;
    public float stayDuration = 5f;
    public float rollOutDuration = 3f;
    public float waveWaitDuration = 5f;

    [Header("Shell Spawning")]
    [SerializeField] private GameObject[] shellPrefabs;
    public bool spawning = true;

    [Header("Drag Settings")]
    public float dragStrength = 1f;
    public LayerMask draggableLayers;

    public bool tideRolling = true;

    private List<Rigidbody> caughtObjects = new List<Rigidbody>();
    private Vector3 lastPosition;

    private void Start()
    {
        StartCoroutine(TideCycle());
    }

    public IEnumerator TideCycle()
    {
        while (tideRolling)
        {
            // Roll In
            float t = 0;
            lastPosition = transform.position;
            while (t < rollInDuration)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(startPoint.position, inPoint.position, Mathf.SmoothStep(0, 1, t / rollInDuration));
                yield return null;
            }

            // Stay
            yield return new WaitForSeconds(stayDuration);

            // Roll Out — drag caught objects
            t = 0;
            lastPosition = transform.position;
            while (t < rollOutDuration)
            {
                t += Time.deltaTime;
                Vector3 newPosition = Vector3.Lerp(inPoint.position, startPoint.position, Mathf.SmoothStep(0, 1, t / rollOutDuration));

                Vector3 tideMovement = newPosition - lastPosition;
                lastPosition = newPosition;
                transform.position = newPosition;

                foreach (Rigidbody rb in caughtObjects)
                {
                    if (rb == null) continue;
                    rb.MovePosition(rb.position + tideMovement * dragStrength);
                }

                yield return null;
            }

            caughtObjects.Clear();
            yield return new WaitForSeconds(waveWaitDuration);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & draggableLayers) == 0) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && !caughtObjects.Contains(rb))
        {
            caughtObjects.Add(rb);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            caughtObjects.Remove(rb);
        }
    }
}