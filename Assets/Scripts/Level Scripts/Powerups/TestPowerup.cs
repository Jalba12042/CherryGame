using UnityEngine;

public class SpeedPowerup : Powerup
{
    [SerializeField] private float speedMultiplier;
    private float originalSpeed;
    protected override void powerUpEffect()
    {
        base.powerUpEffect();
        originalSpeed = pc.moveSpeed;
        pc.moveSpeed *= speedMultiplier;
    }

    protected override void powerUpEnd()
    {
        base.powerUpEnd();
        pc.moveSpeed = originalSpeed;
    }
}
