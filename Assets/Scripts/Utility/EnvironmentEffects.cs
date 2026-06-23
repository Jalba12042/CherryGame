using System.Collections.Generic;
using UnityEngine;

// Script for various effects for the environment

public class EnvironmentEffects : MonoBehaviour
{
    // Send all things up
#nullable enable
    public void bigImpact(float itemJumpForce, Rigidbody? exception)
    {
        int cherryLayer = LayerMask.NameToLayer("Cherry");

        Rigidbody[] allRigidbodies = GameObject.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        foreach (var rb in allRigidbodies)
        {
            if (exception != null && rb == exception) continue;

            PlayerEffects pe = rb.GetComponent<PlayerEffects>();
            if (pe != null && pe.isBig) continue;

            GroundCheck gc = rb.GetComponent<GroundCheck>();
            if (gc != null && gc.isGrounded)
            {
                rb.isKinematic = false;
                rb.WakeUp();

                float upForce = itemJumpForce;        // vertical force
                float horizontalRange = 2f;           // horizontal scatter
                Vector3 force = new Vector3(
                    Random.Range(-horizontalRange, horizontalRange),
                    upForce,
                    Random.Range(-horizontalRange, horizontalRange)
                );

                LevelPickup pickup = rb.GetComponent<LevelPickup>();

                if (pickup)
                {
                    StartCoroutine(pickup.TemporarilyIgnoreBasket(2f));
                }
                rb.AddForce(force, ForceMode.VelocityChange);
            }              
        }
    } 

}
