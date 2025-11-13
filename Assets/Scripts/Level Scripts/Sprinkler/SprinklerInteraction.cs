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

        // --- Use the player's PowerupHandler instead of Playermovement directly ---
        PlayerPowerupHandler handler = other.GetComponent<PlayerPowerupHandler>();
        if (handler == null) return;

        // Apply pushback using the handler
        Vector3 pushDir = (other.transform.position - transform.position).normalized;
        handler.ApplyPushback(pushDir, pushForce);

        // Apply slow effect if not already slowed
        if (!handler.isSlowed)
        {
            handler.ApplySlow(slowMultiplier, slowDuration);
        }
    }
}
