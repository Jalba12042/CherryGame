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
                Vector3 deathPos = other.transform.position;

                pk.killPlayer(true);

                // NEW: Play the bite sound right where the player died
                if (biteKillSound != null)
                {
                    AudioSource.PlayClipAtPoint(biteKillSound, transform.position, 1f);
                }

                Zombie zombieRef = zombiePrefab.GetComponent<Zombie>();

                Vector3 spawnPos = zombieRef.GetGroundPosition(deathPos);
                spawnPos.y += 0.05f;

                Zombie newZombie = Instantiate(zombiePrefab, spawnPos, Quaternion.identity)
                    .GetComponent<Zombie>();

                newZombie.InitAsPlayerZombie();
            }
        }
    }
}