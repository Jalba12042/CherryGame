using UnityEngine;

public class TestPowerup : Powerup
{
    protected override void powerUpEffect()
    {
        pc.speed += 2;
    }
}
