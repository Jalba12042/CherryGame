using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SprinklerController))]
public class SprinklerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float pushForce = 5f;           // Force applied away from sprinkler
    public float slowMultiplier = 0.5f;    // Player moveSpeed multiplier while slowed
    public float slowDuration = 2f;        // How long player is slowed

    private SprinklerController sprinkler;

    private void Awake()
    {
        sprinkler = GetComponent<SprinklerController>();
    }

    private void OnTriggerStay(Collider other)
    {
        // Only affect players when sprinkler is active
        if (!sprinkler.IsActive()) return;

        // Only affect objects tagged as "Player"
        if (!other.CompareTag("Player")) return;

        // Apply pushback if player has Rigidbody
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 pushDir = (other.transform.position - transform.position).normalized;
            rb.AddForce(pushDir * pushForce, ForceMode.VelocityChange);
        }

        // Apply slow effect if player has PlayerMovement component
        Playermovement player = other.GetComponent<Playermovement>();
        if (player != null && !player.IsSlowed)
        {
            player.StartCoroutine(ApplySlow(player));
        }
    }

    private IEnumerator ApplySlow(Playermovement player)
    {
        player.IsSlowed = true;
        float originalSpeed = player.moveSpeed;
        player.moveSpeed *= slowMultiplier;

        yield return new WaitForSeconds(slowDuration);

        player.moveSpeed = originalSpeed;
        player.IsSlowed = false;
    }
}
