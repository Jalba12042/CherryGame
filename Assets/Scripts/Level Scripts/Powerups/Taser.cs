using UnityEngine;

public class Taser : Powerup
{
    protected override void powerUpEffect()
    {
        base.powerUpEffect();
        pe.isTase = true;

    }

    protected override void powerUpEnd()
    {
        base.powerUpEnd();
        pe.isTase = false;

    }
}
