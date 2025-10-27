using UnityEngine;

public class TestPowerup : Powerup
{
    protected override void powerUpEffect()
    {
        pc.moveSpeed += 2;
    }
}
