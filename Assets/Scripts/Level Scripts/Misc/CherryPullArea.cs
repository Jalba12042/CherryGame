using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class CherryPullArea : MonoBehaviour
{
    public Transform snapPoint;
    public float pullSpeed = 12f;      // Use speed instead of force
    public float snapDistance = 0.25f;
    public float maxMagnetTime = 2f;
    public GameObject pullEffect;

    private HashSet<Rigidbody> activeCherries = new HashSet<Rigidbody>();

    /*private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Cherry")) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        if (activeCherries.Contains(rb)) return;

        activeCherries.Add(rb);

        // Enable shader effect
        if (pullEffect != null)
            pullEffect.SetActive(true);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        StartCoroutine(PullCherry(rb));
    }*/

    /*private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Cherry")) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        if (activeCherries.Contains(rb)) return;

        // Only activate if cherry is moving fast enough
        if (rb.linearVelocity.magnitude < 1f) return; // tweak 1f to your “throw speed” threshold

        activeCherries.Add(rb);

        // Enable shader effect
        if (pullEffect != null)
            pullEffect.SetActive(true);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        StartCoroutine(PullCherry(rb));
    }*/

    private void OnTriggerEnter(Collider other)
    {
        Cherry cherry = other.GetComponent<Cherry>();
        if (cherry == null) return;
        if (cherry.ignoreBasketPull) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;
        if (activeCherries.Contains(rb)) return;
        if (rb.linearVelocity.magnitude < 1f) return;

        activeCherries.Add(rb);

        if (pullEffect != null)
            pullEffect.SetActive(true);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        StartCoroutine(PullCherry(rb));
    }

    private IEnumerator PullCherry(Rigidbody rb)
    {
        float timer = 0f;

        while (rb != null)
        {
            Cherry ch = rb.GetComponent<Cherry>();
            if (ch == null) continue;
            if (ch.ignoreBasketPull) continue;

            Vector3 direction = snapPoint.position - rb.position;
            float distance = direction.magnitude;

            if (distance <= snapDistance)
            {
                rb.linearVelocity = Vector3.zero;
                rb.position = snapPoint.position;

                rb.angularVelocity = Vector3.zero;
                rb.useGravity = true;

                // Disable shader effect
                if (pullEffect != null)
                    pullEffect.SetActive(false);

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