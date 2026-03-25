using UnityEngine;

public class ZombieAttack : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("YAH GRAH");
            other.GetComponent<PlayerKill>().killPlayer();
        }
    }
}
