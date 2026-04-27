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
    [SerializeField] private float growthModifier;
    [SerializeField] private float newGCOffset;
    private GroundCheck gc;
    private float ogMinPitch;
    private float ogMaxPitch;
    private Scream screamScript;
    private float originalSpeed;
    private Vector3 originalSize;
    private PlayerEffects playerEffects;
    private float originalGCDist;
    private float originalGCOffset;

    // --- NEW: PHYSICS VARIABLES FOR JUMP DETECTION ---
    private Rigidbody rb;
    private bool wasGrounded;

    [Header("Audio")]
    public AudioClip growSound;
    public AudioClip shrinkSound;
    public AudioClip bigJumpSound; // NEW: Drag your heavy jump sound here!
    private AudioSource fxSource;

    protected override void powerUpEffect()
    {
        base.powerUpEffect();

        gc = playerModel.GetComponent<GroundCheck>();
        playerEffects = playerModel.GetComponent<PlayerEffects>();
        screamScript = playerModel.GetComponentInChildren<Scream>();

        // Grab the Rigidbody to track jumping upward momentum
        rb = pc.GetComponent<Rigidbody>();
        if (rb == null) rb = playerModel.GetComponentInParent<Rigidbody>();

        fxSource = gameObject.AddComponent<AudioSource>();
        fxSource.playOnAwake = false;

        if (screamScript != null && screamScript.aSource != null)
        {
            fxSource.outputAudioMixerGroup = screamScript.aSource.outputAudioMixerGroup;
        }

        if (growSound != null)
        {
            fxSource.clip = growSound;
            fxSource.pitch = 1.0f;
            fxSource.Play();
        }

        // --- NEW CLEAN UI LOGIC ---
        if (FaceCamManager.Instance != null) FaceCamManager.Instance.ShowPowerUp(pc.playerIndex, "Protein");

        originalSpeed = pc.moveSpeed;
        originalSize = playerModel.transform.localScale;

        originalGCDist = gc.groundCheckDistance;
        gc.groundCheckDistance *= growthModifier;
        originalGCOffset = gc.groundCheckOffset.y;
        gc.groundCheckOffset.y = newGCOffset;

        if (screamScript != null)
        {
            ogMinPitch = screamScript.minPitch;
            ogMaxPitch = screamScript.maxPitch;

            screamScript.minPitch = grownMinPitch;
            screamScript.maxPitch = grownMaxPitch;
        }

        pc.moveSpeed *= speedMultiplier;

        // Assume we start grounded when they pick it up
        wasGrounded = true;

        StartCoroutine(Grow());
    }

    // --- NEW: JUMP DETECTION LOOP ---
    private void Update()
    {
        if (gc == null || rb == null || playerEffects == null) return;

        // Only check for jumps if the player is currently BIG
        if (playerEffects.isBig)
        {
            // NOTE: Assuming your GroundCheck script uses a boolean named "isGrounded". 
            // If it's named something else (like "grounded"), just change it below!
            bool currentlyGrounded = gc.isGrounded;

            // If we WERE on the ground, but now we are NOT, AND we are moving UP (Velocity Y > 0)
            if (wasGrounded && !currentlyGrounded && rb.linearVelocity.y > 0.1f)
            {
                PlayBigJumpSound();
            }

            wasGrounded = currentlyGrounded;
        }
    }

    private void PlayBigJumpSound()
    {
        if (bigJumpSound != null && fxSource != null)
        {
            // PlayOneShot allows multiple jumps to overlap naturally without cutting each other off
            fxSource.PlayOneShot(bigJumpSound, 1.0f);
        }
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

            if (fxSource != null && fxSource.isPlaying)
            {
                fxSource.pitch = Mathf.Lerp(1.0f, 1.5f, elapsed / growthTime);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerModel.transform.localScale = targetSize;
        if (playerEffects != null) playerEffects.isBig = true;
    }

    private IEnumerator Shrink()
    {
        if (shrinkSound != null && fxSource != null)
        {
            fxSource.clip = shrinkSound;
            fxSource.pitch = 1.5f;
            fxSource.Play();
        }

        // --- NEW CLEAN UI LOGIC ---
        if (FaceCamManager.Instance != null) FaceCamManager.Instance.HidePowerUp(pc.playerIndex);

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

            if (fxSource != null && fxSource.isPlaying)
            {
                fxSource.pitch = Mathf.Lerp(1.5f, 0.8f, elapsed / growthTime);
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
        gc.groundCheckDistance = originalGCDist;
        gc.groundCheckOffset.y = originalGCOffset;

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