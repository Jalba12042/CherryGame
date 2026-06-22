using UnityEngine;

public class TacoProjectile : MonoBehaviour
{
    [SerializeField] private float stunDuration = 10f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerPowerupHandler handler =
            other.GetComponentInParent<PlayerPowerupHandler>();

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