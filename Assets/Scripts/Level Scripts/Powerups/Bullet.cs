using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 1f;

    [Header("Stat Tracker Info")]
    public int shooterID = -1; // -1 means no one has claimed it yet

    private void OnTriggerEnter(Collider other)
    {
        PlayerKill pk = other.GetComponentInParent<PlayerKill>();

        if (pk != null)
        {
            // Optional Safety: Stop players from getting a kill point for shooting themselves!
            Playermovement hitPlayer = pk.GetComponent<Playermovement>();
            if (hitPlayer != null && hitPlayer.playerIndex == shooterID)
            {
                return; // Ignore the collision if it's the shooter
            }

            // Make sure we only count the kill if they aren't already dead
            if (!pk.currDead)
            {
                // --- THE REPORTER ---
                // Tell the StatTracker that the shooter got a kill!
                if (StatTracker.Instance != null && shooterID != -1)
                {
                    StatTracker.Instance.AddPistolKill(shooterID);
                }

                pk.killPlayer(true);
            }

            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}