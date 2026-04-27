using System.Collections.Generic;
using UnityEngine;

public class RippleHitDetector : MonoBehaviour
{
    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    [Header("Hit Settings")]
    public float hitRadius = 0.12f;
    public float pushForce = 18f;
    public float upwardBoost = 2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip splashHitSound; // Plays when someone gets blasted!

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            particles = new ParticleSystem.Particle[ps.main.maxParticles];
        }
    }

    private void Update()
    {
        if (ps == null) return;

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

                Vector3 pushDir = (hit.transform.position - transform.position);
                pushDir.y = 0f;
                pushDir = pushDir.normalized;

                // ---------- PLAYER ----------
                Playermovement player = hit.GetComponentInParent<Playermovement>();
                if (player != null)
                {
                    if (!player.isKnockedBack)
                    {
                        rb.linearVelocity = pushDir * pushForce;
                        player.isKnockedBack = true;
                        StartCoroutine(EndKnockback(player, 0.25f));

                        Projectile proj = player.GetComponent<Projectile>();
                        if (proj != null)
                        {
                            proj.ForceResetCherry();
                        }

                        // --- PLAY HIT SOUND ---
                        if (audioSource != null && splashHitSound != null)
                        {
                            audioSource.PlayOneShot(splashHitSound);
                        }
                    }
                    continue;
                }

                // ---------- ZOMBIE ----------
                Zombie zombie = hit.GetComponentInParent<Zombie>();
                if (zombie != null)
                {
                    rb.linearVelocity = pushDir * pushForce;

                    // --- PLAY HIT SOUND ---
                    if (audioSource != null && splashHitSound != null)
                    {
                        audioSource.PlayOneShot(splashHitSound);
                    }
                }
            }
        }
    }

    private System.Collections.IEnumerator EndKnockback(Playermovement player, float time)
    {
        yield return new WaitForSeconds(time);
        if (player != null)
        {
            player.isKnockedBack = false;
        }
    }
}