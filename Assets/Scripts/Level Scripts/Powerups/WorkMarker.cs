using UnityEngine;
using System.Collections;

public class WormMarker : MonoBehaviour
{
    bool active = false;

    public void StartCountdown(float delay)
    {
        if (active) return;

        active = true;

        Debug.Log("Worms attached to: " + gameObject.name);

        StartCoroutine(WormRoutine(delay));
    }

    IEnumerator WormRoutine(float delay)
    {
        // PLACE VISUAL EFFECT HERE
        // Example: shader swap, shake, particles, etc.

        yield return new WaitForSeconds(delay);

        Debug.Log("Worms destroyed: " + gameObject.name);

        Destroy(gameObject);
    }
}