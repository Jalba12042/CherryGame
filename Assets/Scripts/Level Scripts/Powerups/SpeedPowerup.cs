using UnityEngine;

public class SpeedPowerup : Powerup
{
    [SerializeField] private float speedMultiplier;
    private float originalSpeed;

    [Header("Audio")]
    public AudioClip pickupSound;

    protected override void powerUpEffect()
    {
        base.powerUpEffect();
        originalSpeed = pc.moveSpeed;
        pc.moveSpeed *= speedMultiplier;

        if (pickupSound != null)
        {
            Vector3 soundPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(pickupSound, soundPos, 1f);
        }

        // --- NEW CLEAN UI LOGIC ---
        if (FaceCamManager.Instance != null) FaceCamManager.Instance.ShowPowerUp(pc.playerIndex, "Coffee");
    }

    protected override void powerUpEnd()
    {
        base.powerUpEnd();
        pc.moveSpeed = originalSpeed;

        // --- NEW CLEAN UI LOGIC ---
        if (FaceCamManager.Instance != null) FaceCamManager.Instance.HidePowerUp(pc.playerIndex);

        Destroy(gameObject);
    }

    protected override void passOldPowerupInfo(Powerup oldPu)
    {
        SpeedPowerup powerup = (SpeedPowerup)oldPu;
        this.originalSpeed = powerup.originalSpeed;
    }
}