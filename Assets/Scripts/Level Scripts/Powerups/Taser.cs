using UnityEngine;

public class Taser : Powerup
{
    protected override void powerUpEffect()
    {
        base.powerUpEffect();
        powerupHandler.isTased = true;
    }

    protected override void powerUpEnd()
    {
        base.powerUpEnd();
        powerupHandler.isTased = false;
    }
}
