using UnityEngine;

public class Killzone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerKill pk = other.GetComponent<PlayerKill>();
        if (pk != null)
            pk.killPlayer(false);
    }
}
