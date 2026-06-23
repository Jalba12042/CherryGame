/*using UnityEngine;
using System.Collections;

public class CanOfWormsEffect : Powerup
{
    [Header("Targeting")]
    public string targetTag = "Cherry";

    [Header("Infection Settings")]
    public float destroyDelay = 3f;
    public float spreadRadius = 2f;
    public float spreadDelay = 1f;
    public int maxInfections = 5;

    protected override void powerUpEffect()
    {
        ActivateWorms();

        // instant-use cleanup
        powerupHandler.currPowerups[powerUpID] = false;

        if (powerupHandler.activePowerupInstances[powerUpID] == this)
        {
            powerupHandler.activePowerupInstances[powerUpID] = null;
        }

        Destroy(gameObject);
    }

    protected override IEnumerator StartTimer()
    {
        yield break;
    }

    void ActivateWorms()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);

        if (targets.Length == 0)
        {
            Debug.Log("No valid targets found.");
            return;
        }

        GameObject chosenTarget = targets[Random.Range(0, targets.Length)];

        Debug.Log("Infecting: " + chosenTarget.name);

        StartInfection(chosenTarget);
    }

    void StartInfection(GameObject target)
    {
        // Prevent double infection
        if (target.GetComponent<Infection>() != null) return;

        Infection infection = target.AddComponent<Infection>();
        infection.Init(destroyDelay, spreadRadius, spreadDelay, maxInfections, targetTag);
    }
}

//INTERNAL infection behavior (replaces WormMarker)
public class Infection : MonoBehaviour
{
    bool active = false;

    float destroyDelay;
    float spreadRadius;
    float spreadDelay;
    int maxInfections;
    int currentInfections = 0;
    string targetTag;

    public void Init(float delay, float radius, float spreadDelay, int maxInfections, string tag)
    {
        if (active) return;

        active = true;

        this.destroyDelay = delay;
        this.spreadRadius = radius;
        this.spreadDelay = spreadDelay;
        this.maxInfections = maxInfections;
        this.targetTag = tag;

        Debug.Log("Infected: " + gameObject.name);

        StartCoroutine(WormRoutine());
        StartCoroutine(SpreadRoutine());
    }

    IEnumerator WormRoutine()
    {
        // visual feedback
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.green;
        }

        yield return new WaitForSeconds(destroyDelay);

        Debug.Log("Destroyed: " + gameObject.name);
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
                if (col.CompareTag(targetTag))
                {
                    if (col.GetComponent<Infection>() == null)
                    {
                        Infection newInf = col.gameObject.AddComponent<Infection>();
                        newInf.Init(destroyDelay, spreadRadius, spreadDelay, maxInfections, targetTag);

                        currentInfections++;
                        break; // infect one at a time
                    }
                }
            }
        }
    }
}*/