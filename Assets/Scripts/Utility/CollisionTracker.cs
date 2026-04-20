using UnityEngine;

// Use CollisionBroadcaster.cs and CollisionTracker.cs if you want to track collisions from OTHER gameobjects
// Simply attach the tracker to a gameobject that needs to track the broadcaster
// OR: attach this code to whatever needs tracking
public class CollisionTracker : MonoBehaviour
{
    public CollisionBroadcaster target;

    void OnEnable()
    {
        target.OnCollisionEntered += HandleCollision;
    }

    void OnDisable()
    {
        target.OnCollisionEntered -= HandleCollision;
    }

    void HandleCollision(Collision collision)
    {
        Debug.Log("Target collided with: " + collision.gameObject.name);
    }
}
