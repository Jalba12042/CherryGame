using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class CherryPullArea : MonoBehaviour
{
    public Transform snapPoint;
    public float pullSpeed = 12f;      // Use speed instead of force
    public float snapDistance = 0.25f;
    public float maxMagnetTime = 2f;

    private HashSet<Rigidbody> activeCherries = new HashSet<Rigidbody>();

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Cherry")) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        // Prevent double magnet pulls
        if (activeCherries.Contains(rb)) return;

        activeCherries.Add(rb);

        // IMPORTANT: Kill forward momentum immediately
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Use continuous collision for safety
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        StartCoroutine(PullCherry(rb));
    }

    private IEnumerator PullCherry(Rigidbody rb)
    {
        float timer = 0f;

        while (rb != null)
        {
            Vector3 direction = snapPoint.position - rb.position;
            float distance = direction.magnitude;

            if (distance <= snapDistance)
            {
                rb.linearVelocity = Vector3.zero;
                //rb.isKinematic = true;
                rb.position = snapPoint.position;

                rb.angularVelocity = Vector3.zero;
                rb.useGravity = true;

                activeCherries.Remove(rb);
                yield break;
            }

            // Move smoothly toward snap point (no physics push)
            Vector3 move = direction.normalized * pullSpeed * Time.deltaTime;
            rb.MovePosition(rb.position + move);

            timer += Time.deltaTime;
            if (timer > maxMagnetTime)
            {
                activeCherries.Remove(rb);
                yield break;
            }

            yield return null;
        }
    }
}