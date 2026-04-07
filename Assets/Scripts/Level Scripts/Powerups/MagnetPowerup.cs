using UnityEngine;
using System.Collections;

public class MagnetPowerup : Powerup
{
    [Header("Magnet Settings")]
    public string targetTag = "Collectible";
    public float pullRadius = 5f;
    public float pullForce = 10f;

    [Header("Attachment")]
    public string handPointName = "MagnetPoint"; // name of child object on player

    private Coroutine magnetRoutine;
    private Transform handPoint;

    protected override void powerUpEffect()
    {
        base.powerUpEffect();

        AttachToHand();

        magnetRoutine = StartCoroutine(MagnetRoutine());
    }

    protected override void powerUpEnd()
    {
        base.powerUpEnd();

        if (magnetRoutine != null)
        {
            StopCoroutine(magnetRoutine);
        }

        // Optional: destroy or detach visual
        Destroy(gameObject);
    }

    void AttachToHand()
    {
        // Find hand point on player
        handPoint = FindChildRecursive(playerModel.transform, handPointName);

        if (handPoint == null)
        {
            Debug.LogWarning("Hand point not found!");
            return;
        }

        // Parent this powerup to the hand
        transform.SetParent(handPoint);

        // Reset local transform
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    IEnumerator MagnetRoutine()
    {
        while (true)
        {
            Collider[] hits = Physics.OverlapSphere(playerModel.transform.position, pullRadius);

            foreach (Collider hit in hits)
            {
                if (hit.CompareTag(targetTag))
                {
                    Rigidbody rb = hit.GetComponent<Rigidbody>();

                    if (rb != null)
                    {
                        Vector3 direction = (playerModel.transform.position - hit.transform.position).normalized;
                        rb.AddForce(direction * pullForce, ForceMode.Acceleration);
                    }
                }
            }

            yield return null;
        }
    }

    // Recursive search helper
    Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindChildRecursive(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
}