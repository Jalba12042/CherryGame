using UnityEngine;

public class TacoProjectile : MonoBehaviour
{
    [SerializeField] private float stunDuration = 10f;

    private PlayerPowerupHandler owner;

    public void SetOwner(PlayerPowerupHandler player)
    {
        owner = player;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerPowerupHandler handler =
            other.GetComponentInParent<PlayerPowerupHandler>();

        // Ignore the player who fired this projectile
        if (handler != null && handler == owner)
        {
            return;
        }

        // Hit another player
        if (handler != null)
        {
            handler.ApplyTacoStun(stunDuration);
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}