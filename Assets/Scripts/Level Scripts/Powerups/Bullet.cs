using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 1f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerKill pk = other.GetComponentInParent<PlayerKill>();

        if (pk != null)
        {
            pk.killPlayer(true);
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}