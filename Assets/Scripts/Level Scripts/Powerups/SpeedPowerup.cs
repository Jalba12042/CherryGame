using UnityEngine;

public class SpeedPowerup : Powerup
{
    [SerializeField] private float speedMultiplier;
    private float originalSpeed;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource; // NEW: Drag the AudioSource here!
    public AudioClip pickupSound;
    [Range(0f, 1f)] public float pickupVolume = 1f; // NEW: Easy volume slider

    protected override void powerUpEffect()
    {
        base.powerUpEffect();
        originalSpeed = pc.moveSpeed;
        pc.moveSpeed *= speedMultiplier;

        // --- NEW AUDIO LOGIC ---
        if (audioSource != null && pickupSound != null)
        {
            // We use PlayOneShot so it doesn't get interrupted if multiple things happen
            audioSource.PlayOneShot(pickupSound, pickupVolume);
        }
        else if (pickupSound != null)
        {
            // Fallback just in case you forgot to drag the AudioSource in
            Vector3 soundPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(pickupSound, soundPos, pickupVolume);
        }

        // --- NEW CLEAN UI LOGIC ---
        if (FaceCamManager.Instance != null) FaceCamManager.Instance.ShowPowerUp(pc.playerIndex, "Coffee");
    }

    protected override void powerUpEnd()
    {
        base.powerUpEnd();
        pc.moveSpeed = originalSpeed;

        if (FaceCamManager.Instance != null) FaceCamManager.Instance.HidePowerUp(pc.playerIndex);

        // --- THE "NERF GUN" FIX ---
        // If we destroy the object immediately, the sound cuts off.
        // We hide the visuals first, wait for the sound length, then destroy.
        HideVisuals();
        Destroy(gameObject, pickupSound != null ? pickupSound.length : 0.1f);
    }

    private void HideVisuals()
    {
        // Turn off renderers so it "disappears" while sound finishes
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) r.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    protected override void passOldPowerupInfo(Powerup oldPu)
    {
        SpeedPowerup powerup = (SpeedPowerup)oldPu;
        this.originalSpeed = powerup.originalSpeed;
    }
}