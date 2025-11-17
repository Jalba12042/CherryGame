using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SprinklerController))]
public class SprinklerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float pushForce = 5f;           // Impulse force applied away from sprinkler
    public float slowMultiplier = 0.5f;    // Movement speed multiplier while slowed
    public float slowDuration = 2f;        // How long player is slowed

    private SprinklerController sprinkler;

    private void Awake()
    {
        sprinkler = GetComponent<SprinklerController>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!sprinkler.IsActive()) return;
        if (!other.CompareTag("Player")) return;

        // --- PUSH FORCE (Impulse) ---
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 pushDir = (other.transform.position - transform.position).normalized;
            rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
        }

        // --- SLOW EFFECT ON PlayerMovement ---
        Playermovement pm = other.GetComponent<Playermovement>();
        if (pm != null && !pm.isSlowed)
        {
            StartCoroutine(ApplySlow(pm));
        }
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
