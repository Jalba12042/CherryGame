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

    private Rigidbody rb;
    private bool wasGrounded;

    [Header("Audio")]
    [SerializeField] private AudioSource fxSource; // NEW: Slot for your AudioSource component!
    public AudioClip growSound;
    public AudioClip shrinkSound;
    public AudioClip bigJumpSound;
    [Range(0f, 1f)] public float masterPowerupVolume = 1f; // NEW: Volume slider

    protected override void powerUpEffect()
    {
        base.powerUpEffect();

        gc = playerModel.GetComponent<GroundCheck>();
        playerEffects = playerModel.GetComponent<PlayerEffects>();
        screamScript = playerModel.GetComponentInChildren<Scream>();

        rb = pc.GetComponent<Rigidbody>();
        if (rb == null) rb = playerModel.GetComponentInParent<Rigidbody>();

        // --- THE FIX: No longer adding a hidden component via code ---
        if (fxSource != null)
        {
            fxSource.playOnAwake = false;

            // Auto-route to the scream mixer if you forgot to set it up
            if (screamScript != null && screamScript.aSource != null && fxSource.outputAudioMixerGroup == null)
            {
                fxSource.outputAudioMixerGroup = screamScript.aSource.outputAudioMixerGroup;
            }

            if (growSound != null)
            {
                fxSource.clip = growSound;
                fxSource.pitch = 1.0f;
                fxSource.volume = masterPowerupVolume;
                fxSource.Play();
            }
        }

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
        wasGrounded = true;

        StartCoroutine(Grow());
    }

    private void Update()
    {
        if (gc == null || rb == null || playerEffects == null) return;

        if (playerEffects.isBig)
        {
            bool currentlyGrounded = gc.isGrounded;

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
            fxSource.PlayOneShot(bigJumpSound, masterPowerupVolume);
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

            if (fxSource != null && fxSource.isPlaying && fxSource.clip == growSound)
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
            fxSource.volume = masterPowerupVolume;
            fxSource.Play();
        }

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

            if (fxSource != null && fxSource.isPlaying && fxSource.clip == shrinkSound)
            {
                fxSource.pitch = Mathf.Lerp(1.5f, 0.8f, elapsed / growthTime);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerModel.transform.localScale = originalSize;
        if (playerEffects != null) playerEffects.isBig = false;

        // Let the shrink sound finish before deleting
        Destroy(gameObject, 0.5f);
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

        // Hide visuals before shrinking away
        HideVisuals();
        StartCoroutine(Shrink());
    }

    private void HideVisuals()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) r.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
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