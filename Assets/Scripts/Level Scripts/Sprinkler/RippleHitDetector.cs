using System.Collections.Generic;
using UnityEngine;

public class RippleHitDetector : MonoBehaviour
{
    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;


    public float hitRadius = 0.12f; // how close a droplet must be to hit the player
    public float pushForce = 18f;
    public float upwardBoost = 2f;


    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[ps.main.maxParticles];
    }

    private void Update()
    {
        int count = ps.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = particles[i].position;

            Vector3 hitPos = pos + Vector3.down * 0.3f;
            Collider[] hits = Physics.OverlapSphere(hitPos, hitRadius);

            foreach (var hit in hits)
            {
                Rigidbody rb = hit.GetComponentInParent<Rigidbody>();
                if (rb == null) continue;

                // Direction AWAY from sprinkler center
                Vector3 pushDir = (hit.transform.position - transform.position);
                pushDir.y = 0f;
                pushDir = pushDir.normalized;

                // ---------- PLAYER ----------
                Playermovement player = hit.GetComponentInParent<Playermovement>();
                if (player != null)
                {
                    rb.linearVelocity = pushDir * pushForce;

                    player.isKnockedBack = true;
                    StartCoroutine(EndKnockback(player, 0.25f));

                    continue; // don’t double-hit as zombie
                }

                // ---------- ZOMBIE ----------
                Zombie zombie = hit.GetComponentInParent<Zombie>();
                if (zombie != null)
                {
                    // IMPORTANT: do NOT disable movement
                    rb.linearVelocity = pushDir * pushForce;
                }
            }
        }
    }

    private System.Collections.IEnumerator EndKnockback(Playermovement player, float time)
    {
        yield return new WaitForSeconds(time);
        player.isKnockedBack = false;
    }
}
