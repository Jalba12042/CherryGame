using UnityEngine;

public class JumpTestPowerup : Powerup
{
    [SerializeField] private float jumpMultiplier;
    private float originalJumpForce;
    protected override void powerUpEffect()
    {
        base.powerUpEffect();
        originalJumpForce = pc.jumpForce;
        pc.jumpForce *= jumpMultiplier;
    }

    protected override void powerUpEnd()
    {
        base.powerUpEnd();
        pc.jumpForce = originalJumpForce;
        Destroy(gameObject);
    }

    protected override void passOldPowerupInfo(Powerup oldPu)
    {
        JumpTestPowerup powerup = (JumpTestPowerup)oldPu;

        this.originalJumpForce = powerup.originalJumpForce;
    }
}
