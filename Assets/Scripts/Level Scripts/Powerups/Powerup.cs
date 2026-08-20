using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [Header("Powerup Type")]
    [SerializeField] protected bool isHoldable = false;

    [SerializeField] protected float duration;
    [SerializeField] protected string puName;
    [SerializeField] protected int despawnTimerInSecs = 7;

    public int powerUpID;

    protected Playermovement pc;
    protected PlayerPowerupHandler powerupHandler;
    protected GameObject playerModel;
    protected PlayerEffects pe;
    protected bool canDespawn = true;

    protected Coroutine activeTimer;
    private bool isActive;
    protected Coroutine despawnRoutine;

    [Header("Despawn Blink")]
    [Tooltip("How long before despawning the blink warning starts — stays fully solid before that.")]
    [SerializeField] private float blinkWarningDuration = 3f;
    [SerializeField] private float blinkStartInterval = 0.5f;  // blink speed right as the warning starts
    [SerializeField] private float blinkEndInterval = 0.05f;   // blink speed right before it despawns
    private Coroutine blinkRoutine;

    private void Awake()
    {
        despawnRoutine = StartCoroutine(despawnTimer());
    }

    public IEnumerator despawnTimer()
    {
        blinkRoutine = StartCoroutine(BlinkWhileDespawning());

        yield return new WaitForSeconds(despawnTimerInSecs);
        if (canDespawn)
        {
            RoundManager.Instance.powerupsInPlay.Remove(gameObject);
            Destroy(gameObject);
        }
    }

    // Stays solid for most of the despawn timer, then blinks faster and faster as it closes in on
    // despawning — same idea as the UFO's tractor beam flash, just delayed to only warn near the end.
    private IEnumerator BlinkWhileDespawning()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr == null) yield break;

        float warningDuration = Mathf.Min(blinkWarningDuration, despawnTimerInSecs);
        float solidTime = despawnTimerInSecs - warningDuration;
        if (solidTime > 0f)
            yield return new WaitForSeconds(solidTime);

        float elapsed = 0f;
        while (elapsed < warningDuration)
        {
            float progress = warningDuration > 0f ? Mathf.Clamp01(elapsed / warningDuration) : 1f;
            float interval = Mathf.Lerp(blinkStartInterval, blinkEndInterval, progress);

            mr.enabled = !mr.enabled;
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        mr.enabled = true;
        blinkRoutine = null;
    }

    public void Activate(PlayerPowerupHandler handler)
    {
        StopDespawn();

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
            if (!isHoldable)
            {
                Collider col = GetComponent<Collider>();
                if (col != null) col.enabled = false;

                MeshRenderer mr = GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = false;
            }
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
        if (this == null || gameObject == null) return;

        if (activeTimer != null)
        {
            StopCoroutine(activeTimer);
            activeTimer = null;
        }

        if (isActive)
        {
            powerUpEnd();
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
        if (powerupHandler != null)
        {
            if (powerUpID < powerupHandler.currPowerups.Count)
                powerupHandler.currPowerups[powerUpID] = false;

            if (powerupHandler.activePowerupInstances != null &&
                powerupHandler.activePowerupInstances.ContainsKey(powerUpID) &&
                powerupHandler.activePowerupInstances[powerUpID] == this)
            {
                if (powerupHandler.activePowerupInstances.ContainsKey(powerUpID))
                {
                    powerupHandler.activePowerupInstances.Remove(powerUpID);
                }
            }
        }

        Debug.Log($"Powerup ended: {puName} for {pc?.name}");
    }

    void OnEnable()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = true;
        }

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.enabled = true;
        }
    }

    public void StopDespawn()
    {
        canDespawn = false;

        if (despawnRoutine != null)
        {
            StopCoroutine(despawnRoutine);
            despawnRoutine = null;
        }

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;

            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = true;
        }
    }
}
