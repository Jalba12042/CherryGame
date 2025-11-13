using System.Collections;
using UnityEngine;

public class GrowPowerup : Powerup
{
    [SerializeField] private float scaleMultiplier;
    [SerializeField] private float speedMultiplier;
    [SerializeField] private float growthTime;
    [SerializeField] private float grownMinPitch;
    [SerializeField] private float grownMaxPitch;
    [SerializeField] private float pitchTime;
    private float ogMinPitch;
    private float ogMaxPitch;
    private Scream screamScript;
    private float originalSpeed;
    private Vector3 originalSize;
    private PlayerEffects playerEffects;

    protected override void powerUpEffect()
    {
        base.powerUpEffect();

        playerEffects = playerModel.GetComponent<PlayerEffects>();

        //screamScript = playerModel.GetComponent<Scream>();
        screamScript = playerModel.GetComponentInChildren<Scream>();


        // record original values for speed, size, and scream pitches
        originalSpeed = pc.moveSpeed;
        originalSize = playerModel.transform.localScale;
        ogMinPitch = screamScript.minPitch;
        ogMaxPitch = screamScript.maxPitch;

        // change scream pitches, speed and start changing size
        screamScript.minPitch = grownMinPitch;
        screamScript.maxPitch = grownMaxPitch;
        pc.moveSpeed *= speedMultiplier;
        StartCoroutine(Grow());
    }

    // smoothly grow and change pitch
    private IEnumerator Grow()
    {
        Vector3 targetSize = originalSize * scaleMultiplier;
        float elapsed = 0;

        float randPitch = Random.Range(grownMinPitch, grownMaxPitch);
        while (elapsed < growthTime)
        {
            Debug.Log(playerModel);
            playerModel.transform.localScale = Vector3.Lerp(originalSize, targetSize, elapsed / growthTime);
            screamScript.aSource.pitch = Mathf.Lerp(screamScript.aSource.pitch, randPitch, elapsed / pitchTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerModel.transform.localScale = targetSize;
        //pc.isBig = true;
        if (playerEffects != null)
            playerEffects.isBig = true;
    }

    // smoothly shrink and change pitch
    private IEnumerator Shrink()
    {
        Vector3 startSize = playerModel.transform.localScale;
        float elapsed = 0;

        float randPitch = Random.Range(ogMinPitch, ogMaxPitch);
        while (elapsed < growthTime)
        {
            playerModel.transform.localScale = Vector3.Lerp(startSize, originalSize, elapsed / growthTime);
            screamScript.aSource.pitch = Mathf.Lerp(screamScript.aSource.pitch, randPitch, elapsed / pitchTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerModel.transform.localScale = originalSize;
        //pc.isBig = false;
        if (playerEffects != null)
            playerEffects.isBig = false;
        Destroy(gameObject);
    }

    // set values to original
    protected override void powerUpEnd()
    {
        //screamScript = playerModel.GetComponent<Scream>();
        screamScript = playerModel.GetComponentInChildren<Scream>();
        base.powerUpEnd();
        pc.moveSpeed = originalSpeed;
        screamScript.minPitch = ogMinPitch;
        screamScript.maxPitch = ogMaxPitch;
        StartCoroutine(Shrink());
    }

    protected override void passOldPowerupInfo(Powerup oldPu)
    {
        GrowPowerup powerup = (GrowPowerup)oldPu;

        this.originalSpeed = powerup.originalSpeed;
        this.originalSize = powerup.originalSize;
        this.ogMinPitch = powerup.ogMinPitch;
        this.ogMaxPitch = powerup.ogMaxPitch;
        this.screamScript = powerup.screamScript;
    }
}
