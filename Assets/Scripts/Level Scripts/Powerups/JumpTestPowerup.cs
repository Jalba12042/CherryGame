using UnityEngine;

public class JumpTestPowerup : Powerup
{
    protected override void powerUpEffect()
    {
        pc.jumpForce += 2f;
    }
}
