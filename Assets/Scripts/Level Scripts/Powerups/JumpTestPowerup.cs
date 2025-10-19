using UnityEngine;

public class JumpTestPowerup : Powerup
{
    protected override void powerUpEffect()
    {
        pm.jumpForce += 2f;
    }
}
