using System.Collections;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField] protected float duration;
    [SerializeField] protected string puName;

    public int powerUpID; 

    protected Playermovement pc;
    protected GameObject playerModel;

    private Coroutine activeTimer;

    public void Activate(Playermovement player)
    {
        pc = player;
        playerModel = player.gameObject;

        // If already active, restart it instead of starting a duplicate
        if (pc.currPowerups[powerUpID])
        {
            // Restart the timer
            if (activeTimer != null)
                StopCoroutine(activeTimer);

            activeTimer = StartCoroutine(StartTimer(true));
        }
        else
        {
            // Mark active and start timer fresh
            pc.currPowerups[powerUpID] = true;
            activeTimer = StartCoroutine(StartTimer(false));
        }
    }

    protected virtual IEnumerator StartTimer(bool reset)
    {
        if (pc == null) yield break;

        if (!reset)
        {
            // start effect
            powerUpEffect();

            // visually hide object
            GetComponent<Collider>().enabled = false;
            GetComponent<MeshRenderer>().enabled = false;
        }
        else
        {
            Debug.Log($"{puName} timer reset for {pc.name}");
        }

        float timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        powerUpEnd();
    }

    protected virtual void powerUpEffect()
    {
        pc.currPowerups[powerUpID] = true;
        Debug.Log($"Powerup activated: {puName} for {pc.name}");
    }

    protected virtual void powerUpEnd()
    {
        pc.currPowerups[powerUpID] = false;
        Debug.Log($"Powerup ended: {puName} for {pc.name}");
    }
}
