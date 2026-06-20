using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class PickupPullArea : MonoBehaviour
{
    public Transform snapPoint;
    public float pullSpeed = 12f;      // Use speed instead of force
    public float snapDistance = 0.25f;
    public float maxMagnetTime = 2f;
    public GameObject pullEffect;

    private HashSet<Rigidbody> activePickups = new HashSet<Rigidbody>();

    private void OnTriggerEnter(Collider other)
    {
        LevelPickup pickup = other.GetComponent<LevelPickup>();
        if (pickup == null) return;
        if (pickup.ignoreBasketPull) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;
        if (activePickups.Contains(rb)) return;
        if (rb.linearVelocity.magnitude < 1f) return;

        activePickups.Add(rb);

        if (pullEffect != null)
            pullEffect.SetActive(true);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        StartCoroutine(PullPickup(rb));
    }

    private IEnumerator PullPickup(Rigidbody rb)
    {
        float timer = 0f;

        while (rb != null)
        {
            LevelPickup lp = rb.GetComponent<LevelPickup>();
            if (lp == null || lp.ignoreBasketPull)
            {
                yield return null;
                continue;
            }

            Vector3 direction = snapPoint.position - rb.position;
            float distance = direction.magnitude;

            if (distance <= snapDistance)
            {
                rb.linearVelocity = Vector3.zero;
                rb.position = snapPoint.position;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = true;
                activePickups.Remove(rb);
                DisablePullEffect();
                yield break;
            }

            Vector3 move = direction.normalized * pullSpeed * Time.deltaTime;
            rb.MovePosition(rb.position + move);

            timer += Time.deltaTime;
            if (timer > maxMagnetTime)
            {
                activePickups.Remove(rb);
                DisablePullEffect();
                yield break;
            }

            yield return null;
        }

        DisablePullEffect();
    }

    private void DisablePullEffect()
    {
        if (activePickups.Count == 0 && pullEffect != null)
            pullEffect.SetActive(false);
    }
}