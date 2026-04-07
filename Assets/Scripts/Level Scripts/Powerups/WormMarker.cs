using UnityEngine;
using System.Collections;

public class WormMarker : MonoBehaviour
{
    bool active = false;

    float spreadRadius;
    float spreadDelay;
    int maxInfections;
    int currentInfections = 0;

    public void StartCountdown(float delay, float radius = 2f, float spreadDelay = 1f, int maxInfections = 5)
    {
        if (active) return;

        active = true;

        this.spreadRadius = radius;
        this.spreadDelay = spreadDelay;
        this.maxInfections = maxInfections;

        Debug.Log("Worms attached to: " + gameObject.name);

        StartCoroutine(WormRoutine(delay));
        StartCoroutine(SpreadRoutine());
    }

    IEnumerator WormRoutine(float delay)
    {
        // infected is green
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.green;
        }

        yield return new WaitForSeconds(delay);

        Debug.Log("Worms destroyed: " + gameObject.name);

        Destroy(gameObject);
    }

    IEnumerator SpreadRoutine()
    {
        while (currentInfections < maxInfections)
        {
            yield return new WaitForSeconds(spreadDelay);

            Collider[] nearby = Physics.OverlapSphere(transform.position, spreadRadius);

            foreach (Collider col in nearby)
            {
                if (col.CompareTag("Cherry"))
                {
                    WormMarker other = col.GetComponent<WormMarker>();

                    if (other == null)
                    {
                        other = col.gameObject.AddComponent<WormMarker>();
                        other.StartCountdown(3f, spreadRadius, spreadDelay, maxInfections);

                        currentInfections++;
                        break;
                    }
                }
            }
        }
    }
}
