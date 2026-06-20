using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Tide
{
    RollingInSpawn,
    RollingOutDrag,
    RollingIn,
    RollingOut
}

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
    [SerializeField] private int minSpawns = 1;
    [SerializeField] private int maxSpawns = 5;
    public List<GameObject> currShells;
 
    [Header("Drag Settings")]
    public float dragStrength = 1f;
    public LayerMask draggableLayers;

    public bool tideRolling = true;

    private List<Rigidbody> caughtObjects = new List<Rigidbody>();
    private Vector3 lastPosition;
    private bool roundStarted = false;

    private void Update()
    {
        if (RoundManager.Instance.currRoundActive && !roundStarted)
        {
            roundStarted = true;
            StartCoroutine(TideCycle());
        }
       
    }

    public IEnumerator TideCycle()
    {
        Bounds b = GetComponent<Collider>().bounds;
        while (RoundManager.Instance.currRoundActive)
        {
            // Roll In and Spawn Shells
            int randSpawns = Random.Range(minSpawns, maxSpawns + 1);

            for (int i = 0; i < randSpawns; i++)
            {
                float randX = Random.Range(b.min.x, b.max.x);
                float randZ = Random.Range(b.min.z, b.max.z);

                int randShellIndex = Random.Range(0, shellPrefabs.Length);
                currShells.Add(Instantiate(shellPrefabs[randShellIndex], new Vector3(randX, transform.position.y, randZ), Quaternion.identity));
            }

            float t = 0;
            lastPosition = transform.position;
            while (t < rollInDuration)
            {
                t += Time.deltaTime;
                Vector3 newPosition = Vector3.Lerp(startPoint.position, inPoint.position, Mathf.SmoothStep(0, 1, t / rollInDuration));

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

            // Stay
            yield return new WaitForSeconds(stayDuration);

            // Roll Out � DON'T drag caught objects
            t = 0;
            lastPosition = transform.position;
            while (t < rollOutDuration)
            {
                t += Time.deltaTime;
                Vector3 newPosition = Vector3.Lerp(inPoint.position, startPoint.position, Mathf.SmoothStep(0, 1, t / rollOutDuration));

                lastPosition = newPosition;
                transform.position = newPosition;

                yield return null;
            }

            yield return new WaitForSeconds(waveWaitDuration);

            t = 0;
            lastPosition = transform.position;
            while (t < rollInDuration)
            {
                t += Time.deltaTime;
                Vector3 newPosition = Vector3.Lerp(startPoint.position, inPoint.position, Mathf.SmoothStep(0, 1, t / rollInDuration));

                lastPosition = newPosition;
                transform.position = newPosition;

                yield return null;
            }

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

            for (int i = caughtObjects.Count - 1; i >= 0; i--)
            {
                if (caughtObjects[i] == null) continue;
                PlayerKill pk = caughtObjects[i].GetComponent<PlayerKill>();
                if (pk != null)
                    pk.killPlayer();
                else
                    Destroy(caughtObjects[i].gameObject);
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