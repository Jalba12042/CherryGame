using UnityEngine;

public class TestPowerup : Powerup
{
    protected override void powerUpEffect()
    {
        pm.moveSpeed += 2;
    }
}
