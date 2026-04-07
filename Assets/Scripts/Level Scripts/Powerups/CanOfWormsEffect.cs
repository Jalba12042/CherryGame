using UnityEngine;
using System.Collections;

public class CanOfWormsEffect : Powerup
{
    public string targetTag = "Cherry";
    public float destroyDelay = 3f;

    public float spreadRadius = 2f;
    public float spreadDelay = 1f;
    public int maxInfections = 5;

    protected override void powerUpEffect()
    {
        base.powerUpEffect();

        ActivateWorms();

        // single use end
        powerUpEnd();
    }

    protected override IEnumerator StartTimer()
    {
        yield break; // durantion
    }

    void ActivateWorms()
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

        marker.StartCountdown(destroyDelay, spreadRadius, spreadDelay, maxInfections);
    }
}
