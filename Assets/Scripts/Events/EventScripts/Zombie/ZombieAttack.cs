using UnityEngine;

public class ZombieAttack : MonoBehaviour
{
    public GameObject zombiePrefab;

    [Header("Audio")]
    public AudioClip biteKillSound; // NEW: The crunchy bite sound

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerKill pk = other.GetComponent<PlayerKill>();
            PlayerEffects pe = other.GetComponent<PlayerEffects>();
            if (!pk.currDead && !pe.isBig)
            {
                pk.killPlayer();

                // NEW: Play the bite sound right where the player died
                if (biteKillSound != null)
                {
                    AudioSource.PlayClipAtPoint(biteKillSound, transform.position, 1f);
                }

                Zombie newZombie = Instantiate(zombiePrefab, transform.position, Quaternion.identity)
                .GetComponent<Zombie>();

                newZombie.InitAsPlayerZombie();
            }
        }
    }
}