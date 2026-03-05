using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SprinklerController))]
public class SprinklerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float pushForce = 5f;
    public float slowMultiplier = 0.5f;
    public float slowDuration = 2f;

    private SprinklerController sprinkler;
    [SerializeField] private SphereCollider waterZone;

    private void Awake()
    {
        sprinkler = GetComponent<SprinklerController>();

        if (waterZone == null)
            waterZone = GetComponentInChildren<SphereCollider>();

        if (waterZone != null)
        {
            waterZone.enabled = false;
            //Debug.Log("Water zone found and disabled at start.");
        }
        else
        {
            // this error crashes the game so i removed it for now:
            //Debug.LogError("NO WATER ZONE FOUND!");
        }
    }

    private void Update()
    {
        if (waterZone == null) return;

        bool active = sprinkler.IsActive();
        waterZone.enabled = active;

        /*if (active)
            Debug.Log("Water zone ENABLED");*/
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger ENTER by: " + other.name);

        if (other.CompareTag("Player"))
            Debug.Log(">>> PLAYER ENTERED WATER ZONE <<<");
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("Trigger STAY by: " + other.name);

        if (!sprinkler.IsActive())
        {
            Debug.Log("Sprinkler NOT active");
            return;
        }

        if (!other.CompareTag("Player"))
        {
            Debug.Log("Not Player");
            return;
        }

        Debug.Log(">>> APPLYING PUSH + SLOW TO PLAYER <<<");

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 pushDir = (other.transform.position - transform.position).normalized;
            rb.AddForce(pushDir * pushForce, ForceMode.Force);
        }
        else
        {
            Debug.LogWarning("Player has NO Rigidbody!");
        }

        Playermovement pm = other.GetComponent<Playermovement>();
        if (pm != null && !pm.isSlowed)
        {
            Debug.Log("Applying slow effect");
            StartCoroutine(ApplySlow(pm));
        }
        else if (pm == null)
        {
            Debug.LogWarning("Player has NO Movement script!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger EXIT by: " + other.name);

        if (other.CompareTag("Player"))
            Debug.Log(">>> PLAYER LEFT WATER ZONE <<<");
    }

    private IEnumerator ApplySlow(Playermovement pm)
    {
        pm.isSlowed = true;
        pm.moveSpeed *= slowMultiplier;

        yield return new WaitForSeconds(slowDuration);

        pm.moveSpeed /= slowMultiplier;
        pm.isSlowed = false;
    }
}
