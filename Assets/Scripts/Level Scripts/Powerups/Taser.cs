using UnityEngine;

public class Taser : Powerup
{
    protected override void powerUpEffect()
    {
        base.powerUpEffect();
        pc.isTase = true;
    }

    protected override void powerUpEnd()
    {
        base.powerUpEnd();
        pc.isTase = false;
    }
}
