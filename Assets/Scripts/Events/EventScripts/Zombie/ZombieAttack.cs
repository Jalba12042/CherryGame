using UnityEngine;

public class ZombieAttack : MonoBehaviour
{
    public GameObject zombiePrefab;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerKill pk = other.GetComponent<PlayerKill>();
            if (!pk.currDead)
            {
                pk.killPlayer();

                Zombie newZombie = Instantiate(zombiePrefab, transform.position, Quaternion.identity)
                .GetComponent<Zombie>();

                newZombie.InitAsPlayerZombie();
            }
        }
    }
}
