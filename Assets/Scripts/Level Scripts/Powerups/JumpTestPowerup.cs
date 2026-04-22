using UnityEngine;

public class JumpTestPowerup : Powerup
{
    [SerializeField] private float jumpMultiplier;
    private float originalJumpForce;

    [Header("Audio")]
    public AudioClip pickupSound;

    protected override void powerUpEffect()
    {
        base.powerUpEffect();
        originalJumpForce = pc.jumpForce;
        pc.jumpForce *= jumpMultiplier;

        if (pickupSound != null)
        {
            Vector3 soundPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(pickupSound, soundPos, 1f);
        }

        // --- NEW CLEAN UI LOGIC ---
        if (FaceCamManager.Instance != null) FaceCamManager.Instance.ShowPowerUp(pc.playerIndex, "Pill");
    }

    protected override void powerUpEnd()
    {
        base.powerUpEnd();
        pc.jumpForce = originalJumpForce;

        // --- NEW CLEAN UI LOGIC ---
        if (FaceCamManager.Instance != null) FaceCamManager.Instance.HidePowerUp(pc.playerIndex);

        Destroy(gameObject);
    }

    protected override void passOldPowerupInfo(Powerup oldPu)
    {
        JumpTestPowerup powerup = (JumpTestPowerup)oldPu;
        this.originalJumpForce = powerup.originalJumpForce;
    }
}