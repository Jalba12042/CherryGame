using UnityEngine;
using UnityEngine.InputSystem;

public class Powerup : MonoBehaviour
{
    protected Playermovement pc;
    private bool playerInRange = false;
    private GameObject playerObj;
    protected virtual void powerUpEffect()
    {
        Debug.Log("powerup");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerObj = other.gameObject;
            pc = playerObj.GetComponent<Playermovement>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerObj = null;
            pc = null;
        }
    }

    private void Update()
    {
        if (playerInRange && Gamepad.current != null)
        {
            // RT is the right trigger, which goes from 0.0 to 1.0 when pressed
            if (Gamepad.current.rightTrigger.wasPressedThisFrame)
            {
                powerUpEffect();
                Destroy(gameObject);
            }
        }
    }

    /*private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            pc = collision.gameObject.GetComponent<Playermovement>();
            powerUpEffect();
            Destroy(gameObject);
        }
            
    }*/
}
