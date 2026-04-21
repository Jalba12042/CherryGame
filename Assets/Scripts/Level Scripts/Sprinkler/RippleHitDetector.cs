using System.Collections.Generic;
using UnityEngine;

public class RippleHitDetector : MonoBehaviour
{
    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    public float hitRadius = 0.12f; // how close a droplet must be to hit the player

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
                PlayerKill pk = hit.GetComponentInParent<PlayerKill>();
                if (pk != null && !pk.currDead)
                {
                    pk.killPlayer();
                }
            }
        }
    }
}
