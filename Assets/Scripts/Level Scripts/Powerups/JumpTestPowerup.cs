using UnityEngine;

public class JumpTestPowerup : Powerup
{
    [SerializeField] private float jumpMultiplier;
    private float originalJumpForce;
    private bool activated = false; // flag to make sure we dont infinitely increase the jump
    protected override void powerUpEffect()
    {
        base.powerUpEffect();
        originalJumpForce = pc.jumpForce;
    }

    private void Update()
    {
        if (pc != null)
        {
            if (active && !activated)
            {
                pc.jumpForce *= jumpMultiplier;
                activated = true;
            }
            else if (!active)
            {
                pc.jumpForce = originalJumpForce;
            }
        }
    }
}
