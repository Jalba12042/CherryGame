using UnityEngine;

public class SpeedPowerup : Powerup
{
    protected override void powerUpEffect()
    {
        pc.moveSpeed *= 2;
    }
}
