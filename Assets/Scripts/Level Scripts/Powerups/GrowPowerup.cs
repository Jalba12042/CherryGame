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

    [Header("Audio")]
    public AudioClip growSound;
    private AudioSource fxSource; // A dedicated source for the powerup sound

    protected override void powerUpEffect()
    {
        base.powerUpEffect();

        playerEffects = playerModel.GetComponent<PlayerEffects>();
        screamScript = playerModel.GetComponentInChildren<Scream>();

        // NEW: Create an AudioSource just for the growing sound
        fxSource = gameObject.AddComponent<AudioSource>();
        fxSource.playOnAwake = false;

        // Auto-route to the same mixer as the player's scream (SFX)
        if (screamScript != null && screamScript.aSource != null)
        {
            fxSource.outputAudioMixerGroup = screamScript.aSource.outputAudioMixerGroup;
        }

        // Play the grow sound
        if (growSound != null)
        {
            fxSource.clip = growSound;
            fxSource.Play();
        }

        originalSpeed = pc.moveSpeed;
        originalSize = playerModel.transform.localScale;

        if (screamScript != null)
        {
            ogMinPitch = screamScript.minPitch;
            ogMaxPitch = screamScript.maxPitch;

            screamScript.minPitch = grownMinPitch;
            screamScript.maxPitch = grownMaxPitch;
        }

        pc.moveSpeed *= speedMultiplier;
        StartCoroutine(Grow());
    }

    private IEnumerator Grow()
    {
        Vector3 targetSize = originalSize * scaleMultiplier;
        float elapsed = 0;

        float randPitch = Random.Range(grownMinPitch, grownMaxPitch);

        while (elapsed < growthTime)
        {
            playerModel.transform.localScale = Vector3.Lerp(originalSize, targetSize, elapsed / growthTime);

            if (screamScript != null && screamScript.aSource != null)
            {
                screamScript.aSource.pitch = Mathf.Lerp(screamScript.aSource.pitch, randPitch, elapsed / pitchTime);
            }

            // NEW: Pitch bend the grow sound DOWN as they get bigger!
            // It starts at 1.2 (slightly high) and bends down to 0.5 (deep and heavy)
            if (fxSource != null && fxSource.isPlaying)
            {
                fxSource.pitch = Mathf.Lerp(1.2f, 0.5f, elapsed / growthTime);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerModel.transform.localScale = targetSize;
        if (playerEffects != null) playerEffects.isBig = true;
    }

    private IEnumerator Shrink()
    {
        Vector3 startSize = playerModel.transform.localScale;
        float elapsed = 0;

        float randPitch = Random.Range(ogMinPitch, ogMaxPitch);
        while (elapsed < growthTime)
        {
            playerModel.transform.localScale = Vector3.Lerp(startSize, originalSize, elapsed / growthTime);

            if (screamScript != null && screamScript.aSource != null)
            {
                screamScript.aSource.pitch = Mathf.Lerp(screamScript.aSource.pitch, randPitch, elapsed / pitchTime);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerModel.transform.localScale = originalSize;
        if (playerEffects != null) playerEffects.isBig = false;
        Destroy(gameObject);
    }

    protected override void powerUpEnd()
    {
        screamScript = playerModel.GetComponentInChildren<Scream>();
        base.powerUpEnd();

        pc.moveSpeed = originalSpeed;

        if (screamScript != null)
        {
            screamScript.minPitch = ogMinPitch;
            screamScript.maxPitch = ogMaxPitch;
        }

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