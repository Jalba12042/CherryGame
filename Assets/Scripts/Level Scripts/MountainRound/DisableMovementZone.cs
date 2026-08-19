using UnityEngine;

// Place just under the tower's platform, below where players can still reach it — once a
// falling player crosses this, they lose horizontal control so gravity carries them straight
// down into the killzone instead of letting them steer back into the tower to cling to it.
public class DisableMovementZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Playermovement pm = other.GetComponent<Playermovement>();
        if (pm != null)
            pm.canMove = false;
    }
}
