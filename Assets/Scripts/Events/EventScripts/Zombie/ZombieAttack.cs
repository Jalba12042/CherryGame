using UnityEngine;

public class ZombieAttack : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerKill>().killPlayer();
        }
    }
}
