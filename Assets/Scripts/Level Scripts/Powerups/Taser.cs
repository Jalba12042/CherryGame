using UnityEngine;

public class Taser : Powerup
{
    protected override void powerUpEffect()
    {
        base.powerUpEffect();
        pe.isTasing = true;
    }

    protected override void powerUpEnd()
    {
        base.powerUpEnd();
        pe.isTasing = false;
    }
}
