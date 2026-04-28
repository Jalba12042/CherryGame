using UnityEngine;
using System.Collections;

public class MagnetPowerup : Powerup
{
    [Header("Magnet Settings")]
    public string targetTag = "Collectible";
    public float pullRadius = 5f;
    public float pullForce = 10f;

    [Header("Attachment")]
    public string handPointName = "MagnetPoint";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip magnetActivateSound; // Plays when the magnet turns on!

    private Coroutine magnetRoutine;
    private Transform handPoint;

    protected override void powerUpEffect()
    {
        base.powerUpEffect();
        AttachToHand();

        // --- PLAY MAGNET SOUND ---
        if (audioSource != null && magnetActivateSound != null)
        {
            audioSource.PlayOneShot(magnetActivateSound);
        }

        magnetRoutine = StartCoroutine(MagnetRoutine());

        // --- NEW CLEAN UI LOGIC ---
        if (FaceCamManager.Instance != null) FaceCamManager.Instance.ShowPowerUp(pc.playerIndex, "Magnet");
    }

    protected override void powerUpEnd()
    {
        base.powerUpEnd();
        if (magnetRoutine != null)
        {
            StopCoroutine(magnetRoutine);
        }

        // --- NEW CLEAN UI LOGIC ---
        if (FaceCamManager.Instance != null) FaceCamManager.Instance.HidePowerUp(pc.playerIndex);

        Destroy(gameObject);
    }

    void AttachToHand()
    {
        handPoint = FindChildRecursive(playerModel.transform, handPointName);
        if (handPoint == null)
        {
            Debug.LogWarning("Hand point not found!");
            return;
        }

        transform.SetParent(handPoint);
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

    Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindChildRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }
}