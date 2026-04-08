using System;
using UnityEngine;

// Use CollisionBroadcaster.cs and CollisionTracker.cs if you want to track collisions from OTHER gameobjects
// Simply attach the tracker to a gameobject that needs to track the broadcaster
public class CollisionBroadcaster : MonoBehaviour
{
    public event Action<Collision> OnCollisionEntered;

    void OnCollisionEnter(Collision collision)
    {
        OnCollisionEntered?.Invoke(collision);
    }
}
