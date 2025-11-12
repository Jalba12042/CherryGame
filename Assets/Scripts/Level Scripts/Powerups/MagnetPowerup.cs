using System.Collections;
using UnityEngine;

public class MagnetPowerup : Powerup
{
    [Header("Magnet Settings")]
    [SerializeField] private string targetTag = "Collectible";
    [SerializeField] private float magnetForce = 10f;
    [SerializeField] private float magnetRange = 5f;
    [SerializeField] private LayerMask attractionMask;

    private bool isMagnetActive;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, magnetRange);
    }

    protected override void powerUpEffect()
    {
        base.powerUpEffect();

        // Start magnet effect
        isMagnetActive = true;
        StartCoroutine(MagnetRoutine());

        Debug.Log($"Magnet Powerup activated for {pc.name}");
    }

    protected override void powerUpEnd()
    {
        base.powerUpEnd();

        // Stop magnet effect
        isMagnetActive = false;
        Debug.Log($"Magnet Powerup ended for {pc.name}");
    }

    private IEnumerator MagnetRoutine()
    {
        while (isMagnetActive)
        {
            PullObjects();
            yield return null;
        }
    }

    private void PullObjects()
    {
        if (pc == null) return;

        Vector3 playerPos = playerModel.transform.position;
        Collider[] colliders = Physics.OverlapSphere(
            playerPos,
            magnetRange,
            attractionMask.value == 0 ? ~0 : attractionMask
        );

        foreach (Collider col in colliders)
        {
            if (!col.CompareTag(targetTag))
                continue;

            Rigidbody rb = col.attachedRigidbody;
            if (rb == null)
                continue;

            Vector3 direction = (playerPos - col.transform.position).normalized;
            float distance = Vector3.Distance(playerPos, col.transform.position);
            float force = magnetForce / Mathf.Max(distance, 0.1f);

            rb.AddForce(direction * force, ForceMode.Acceleration);
        }
    }
}
