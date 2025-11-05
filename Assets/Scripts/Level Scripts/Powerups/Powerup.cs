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
    private bool isActive;
    public void Activate(Playermovement player)
    {
        pc = player;
        playerModel = player.gameObject;

        if (!isActive)
        {
            isActive = true;

            // remove it from the power ups that are in play
            RoundManager.Instance.powerupsInPlay.Remove(gameObject); 
            
            // visually hide gameobject
            GetComponent<Collider>().enabled = false;
            GetComponent<MeshRenderer>().enabled = false;
        }

        if (activeTimer != null)
        {
            StopCoroutine(activeTimer);
        }

        if (pc.currPowerups[powerUpID])
        {
            // Find and stop the old powerup instance
            Powerup oldPowerup = pc.activePowerupInstances[powerUpID];
            if (oldPowerup != null && oldPowerup != this)
            {
                passOldPowerupInfo(oldPowerup);
                oldPowerup.ForceStop();
            }
            Debug.Log($"{puName} timer reset for {pc.name}");
        }
        else
        {
            pc.currPowerups[powerUpID] = true;
            powerUpEffect();
        }

        // Register this as the active instance
        pc.activePowerupInstances[powerUpID] = this;

        activeTimer = StartCoroutine(StartTimer());
    }

    // Passes old powerup information into new one when its picked up if it is implemented by sub class (implemented out of desperation)
    protected virtual void passOldPowerupInfo(Powerup oldPu)
    {
        
    }

    protected virtual IEnumerator StartTimer()
    {
        if (pc == null) yield break;

        float timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        powerUpEnd();
        isActive = false;
        activeTimer = null;
    }

    public void ForceStop()
    {
        if (activeTimer != null)
        {
            StopCoroutine(activeTimer);
            activeTimer = null;
        }
        isActive = false;
    }

    protected virtual void powerUpEffect()
    {
        pc.currPowerups[powerUpID] = true;
        Debug.Log($"Powerup activated: {puName} for {pc.name}");
    }

    protected virtual void powerUpEnd()
    {
        pc.currPowerups[powerUpID] = false;
        if (pc.activePowerupInstances[powerUpID] == this)
        {
            pc.activePowerupInstances[powerUpID] = null;
        }
        Debug.Log($"Powerup ended: {puName} for {pc.name}");
    }
}
