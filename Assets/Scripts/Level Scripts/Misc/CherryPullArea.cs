using UnityEngine;

public class CherryPullArea : MonoBehaviour
{
    public Transform snapPoint;          // Assign CherrySnapPoint
    public float pullForce = 25f;        // Strength of pull
    public float snapDistance = 0.3f;    // When close enough, snap
    public float maxMagnetTime = 2f;     // Safety timeout

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cherry"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                StartCoroutine(PullCherry(rb));
            }
        }
    }

    private System.Collections.IEnumerator PullCherry(Rigidbody rb)
    {
        float timer = 0f;

        while (rb != null)
        {
            Vector3 direction = (snapPoint.position - rb.position);
            float distance = direction.magnitude;

            if (distance <= snapDistance)
            {
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = true;
                rb.position = snapPoint.position;
                yield break;
            }

            rb.AddForce(direction.normalized * pullForce, ForceMode.Acceleration);

            timer += Time.deltaTime;
            if (timer > maxMagnetTime)
                yield break;

            yield return null;
        }
    }
}