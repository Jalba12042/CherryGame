using System.Collections;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField] protected float duration;
    [SerializeField] protected string puName;
    [SerializeField] protected int despawnTimerInSecs = 7;

    public int powerUpID; 

    protected Playermovement pc;
    protected PlayerPowerupHandler powerupHandler;
    protected GameObject playerModel;
    protected PlayerEffects pe;
    protected bool canDespawn = true;

    private Coroutine activeTimer;
    private bool isActive;

    private void Awake()
    {
        StartCoroutine(despawnTimer());
    }

    private IEnumerator despawnTimer()
    {
        yield return new WaitForSeconds(despawnTimerInSecs);
        if (canDespawn)
        {
            RoundManager.Instance.powerupsInPlay.Remove(gameObject);
            Destroy(gameObject);
        }
    }
    public void Activate(PlayerPowerupHandler handler)
    {
        canDespawn = false;
        powerupHandler = handler;
        pc = handler.GetComponent<Playermovement>();
        pe = handler.GetComponent<PlayerEffects>();
        playerModel = handler.gameObject;

        /*pc = player;
        powerupHandler = player.GetComponent<PlayerPowerupHandler>();
        playerModel = player.gameObject;*/

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

        /*if (powerupHandler.currPowerups[powerUpID])
        {
            // Find and stop the old powerup instance
            Powerup oldPowerup = powerupHandler.activePowerupInstances[powerUpID];
            if (oldPowerup != null && oldPowerup != this)
            {
                passOldPowerupInfo(oldPowerup);
                oldPowerup.ForceStop();
            }
            Debug.Log($"{puName} timer reset for {pc.name}");
        }*/
        if (powerupHandler.currPowerups[powerUpID])
        {
            Powerup oldPowerup = powerupHandler.activePowerupInstances[powerUpID];
            if (oldPowerup != null)
            {
                oldPowerup.ResetTimer(duration);
                Destroy(gameObject); // destroy the new pickup
                return;
            }
        }
        else
        {
            powerupHandler.currPowerups[powerUpID] = true;
            powerUpEffect();
        }

        // Register this as the active instance
        powerupHandler.activePowerupInstances[powerUpID] = this;

        activeTimer = StartCoroutine(StartTimer());
    }

    // Passes old powerup information into new one when its picked up if it is implemented by sub class (implemented out of desperation)
    protected virtual void passOldPowerupInfo(Powerup oldPu)
    {
        
    }

    protected virtual IEnumerator StartTimer()
    {
        if (powerupHandler == null) yield break;

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

    public void ResetTimer(float newDuration)
    {
        if (activeTimer != null)
        {
            StopCoroutine(activeTimer);
        }

        activeTimer = StartCoroutine(StartTimer());
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
        powerupHandler.currPowerups[powerUpID] = true;
        Debug.Log($"Powerup activated: {puName} for {pc.name}");
    }

    protected virtual void powerUpEnd()
    {
        powerupHandler.currPowerups[powerUpID] = false;
        if (powerupHandler.activePowerupInstances[powerUpID] == this)
        {
            powerupHandler.activePowerupInstances[powerUpID] = null;
        }
        Debug.Log($"Powerup ended: {puName} for {pc.name}");
    }
}
