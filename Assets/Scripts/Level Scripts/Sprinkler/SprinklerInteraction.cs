using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SprinklerController))]
public class SprinklerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float pushForce = 5f;
    public float disableDuration = 2f;

    private SprinklerController sprinkler;
    [SerializeField] private Collider waterZone;

    private void Awake()
    {
        sprinkler = GetComponent<SprinklerController>();

        if (waterZone == null)
            waterZone = GetComponentInChildren<Collider>();

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


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log(">>> PLAYER ENTERED WATER ZONE <<<");

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 pushDir = (other.transform.position - transform.position).normalized;
            rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
        }

        Playermovement pm = other.GetComponent<Playermovement>();
        if (pm != null)
        {
            pm.enabled = false;
            StartCoroutine(ReenableMovement(pm));
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!sprinkler.IsActive()) return;
        if (!other.CompareTag("Player")) return;
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log("Trigger EXIT by: " + other.name);

        if (other.CompareTag("Player"))
            Debug.Log(">>> PLAYER LEFT WATER ZONE <<<");
    }

    private IEnumerator ReenableMovement(Playermovement pm)
    {
        yield return new WaitForSeconds(2f);
        pm.enabled = true;
    }
}
