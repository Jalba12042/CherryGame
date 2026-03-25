using UnityEngine;

public class ZombieAttack : MonoBehaviour
{
    public GameObject zombiePrefab;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerKill>().killPlayer();
            Zombie newZombie = Instantiate(zombiePrefab, transform.position, Quaternion.identity).GetComponent<Zombie>();
            newZombie.wasPlayer = true;
        }
    }
}
