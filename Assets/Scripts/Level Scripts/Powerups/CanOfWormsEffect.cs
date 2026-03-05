using UnityEngine;

public class CanOfWormsEffect : MonoBehaviour
{
    public string targetTag = "Collectible";
    public float destroyDelay = 3f;

    public void ActivateWorms()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);

        if (targets.Length == 0)
        {
            Debug.Log("CanOfWorms: No valid targets found.");
            return;
        }

        GameObject chosenTarget = targets[Random.Range(0, targets.Length)];

        Debug.Log("CanOfWorms selected: " + chosenTarget.name);

        WormMarker marker = chosenTarget.GetComponent<WormMarker>();

        if (marker == null)
        {
            marker = chosenTarget.AddComponent<WormMarker>();
        }

        marker.StartCountdown(destroyDelay);
    }
}