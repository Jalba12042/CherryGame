using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class SprinklerInteraction : MonoBehaviour
{
    public float pushForce = 5f;
    public float slowMultiplier = 0.5f;
    public float slowDuration = 2f;

    private ParticleSystem sprinklerParticles;
    private SphereCollider sphereCollider;
    private bool colliderState = false;

    private void Awake()
    {
        sprinklerParticles = GetComponentInChildren<ParticleSystem>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        if (sprinklerParticles == null || sphereCollider == null) return;

        // Instantly reflect whether particles are currently being emitted
        bool shouldEnable = sprinklerParticles.isEmitting;

        if (colliderState != shouldEnable)
        {
            sphereCollider.enabled = shouldEnable;
            colliderState = shouldEnable;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (sprinklerParticles == null || !sprinklerParticles.isEmitting) return;
        if (!other.CompareTag("Player")) return;

        // Pushback
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            Vector3 pushDir = (other.transform.position - transform.position).normalized;
            rb.AddForce(pushDir * pushForce, ForceMode.VelocityChange);
        }

        // Slow movement if player has PlayerMovement script
        WASDtester player = other.GetComponent<WASDtester>();
        if (player != null && !player.IsSlowed)
        {
            StartCoroutine(ApplySlow(player));
        }
    }

    private IEnumerator ApplySlow(WASDtester player)
    {
        player.IsSlowed = true;
        float originalSpeed = player.moveSpeed;
        player.moveSpeed *= slowMultiplier;

        yield return new WaitForSeconds(slowDuration);

        player.moveSpeed = originalSpeed;
        player.IsSlowed = false;
    }
}
