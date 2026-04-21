using System.Collections;
using UnityEngine;

public class CherryBomb : Cherry
{
    private bool fuseLit = false;
    [SerializeField] private float fuseTime;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionForce;
    [SerializeField] private float upwardModifier = 1.5f;
    [SerializeField] private LayerMask groundLayer;

    private void Update()
    {
        if (isHeld && !fuseLit)
        {
            fuseLit = true;
            StartCoroutine(LightFuse());
        }
    }

    private IEnumerator LightFuse()
    {
        Debug.Log("OH FUCK CH-CH-CH-CH-CH-CH-CHERRY BOMB"); // change this to our fuse effect
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    private void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider h in hits)
        {
            if ((groundLayer & (1 << h.gameObject.layer)) != 0)
                continue; // skip ground objects

            Rigidbody rb = h.GetComponent<Rigidbody>();
            if (rb == null)
                continue;

            rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardModifier, ForceMode.Impulse);

            PlayerKill pk = rb.GetComponent<PlayerKill>();
            if (pk != null)
                pk.killPlayer();
        }

        Destroy(gameObject);
    }
}