using UnityEngine;
using System.Collections;

public class MagnetPowerup : Powerup
{
    [Header("Magnet Settings")]
    public string targetTag = "Collectible";
    public float pullRadius = 5f;
    public float pullForce = 10f;

    private Coroutine magnetRoutine;

    protected override void powerUpEffect()
    {
        base.powerUpEffect();

        // Start magnet effect
        magnetRoutine = StartCoroutine(MagnetRoutine());
    }

    protected override void powerUpEnd()
    {
        base.powerUpEnd();

        // Stop magnet effect when powerup ends
        if (magnetRoutine != null)
        {
            StopCoroutine(magnetRoutine);
        }
    }

    IEnumerator MagnetRoutine()
    {
        while (true)
        {
            // Find nearby objects
            Collider[] hits = Physics.OverlapSphere(playerModel.transform.position, pullRadius);

            foreach (Collider hit in hits)
            {
                if (hit.CompareTag(targetTag))
                {
                    Rigidbody rb = hit.GetComponent<Rigidbody>();

                    if (rb != null)
                    {
                        Vector3 direction = (playerModel.transform.position - hit.transform.position).normalized;

                        // Apply pull force
                        rb.AddForce(direction * pullForce, ForceMode.Acceleration);
                    }
                }
            }

            yield return null; // every frame
        }
    }
}