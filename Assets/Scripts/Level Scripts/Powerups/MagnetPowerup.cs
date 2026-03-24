using UnityEngine;
using System.Collections;

public class MagnetPowerup : Powerup
{
    [Header("Magnet Settings")]
    [SerializeField] private float magnetRadius = 10f;
    [SerializeField] private float pullForce = 25f;
    [SerializeField] private string targetTag = "Pickup";

    private Coroutine magnetRoutine;

    protected override void powerUpEffect()
    {
        base.powerUpEffect();

        magnetRoutine = StartCoroutine(MagnetLoop());
    }

    private IEnumerator MagnetLoop()
    {
        while (true)
        {
            PullObjects();
            yield return null;
        }
    }

    private void PullObjects()
    {
        if (playerModel == null) return;

        Collider[] hits = Physics.OverlapSphere(playerModel.transform.position, magnetRadius);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag(targetTag)) continue;

            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb == null) continue;

            Vector3 direction = (playerModel.transform.position - hit.transform.position).normalized;
            rb.AddForce(direction * pullForce, ForceMode.Force);
        }
    }

    protected override void powerUpEnd()
    {
        if (magnetRoutine != null)
        {
            StopCoroutine(magnetRoutine);
            magnetRoutine = null;
        }

        base.powerUpEnd();
    }
}