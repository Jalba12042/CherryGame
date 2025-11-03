using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Powerup : MonoBehaviour
{
    protected Playermovement pc;
    protected GameObject playerModel;
    private bool playerInRange = false;

    [SerializeField] protected float duration;
    [SerializeField] protected string puName;
    protected virtual IEnumerator startTimer()
    {
        if (pc == null)
        {
            yield break;
        }

        // start powerup effects
        powerUpEffect();

        // make it look invisible - Victor: make the object drop out of the player's hand here as well
        GetComponent<Collider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;

        yield return new WaitForSeconds(duration);

        powerUpEnd();
    }

    /*private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            playerModel = collision.gameObject;
            pc = collision.gameObject.GetComponent<Playermovement>();
            RoundManager.Instance.powerupsInPlay.Remove(gameObject);
            StartCoroutine(startTimer());
        }
    }*/

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerModel = other.gameObject;
            pc = playerModel.GetComponent<Playermovement>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerModel = null;
            pc = null;
        }
    }


    protected virtual void powerUpEffect()
    {
        Debug.Log($"powerup: {puName}");
    }

    protected virtual void powerUpEnd()
    {
        Debug.Log($"powerup end: {puName}");
    }

    private void Update()
    {
        if (playerInRange && Gamepad.current != null)
        {
            if (Gamepad.current.rightTrigger.wasPressedThisFrame)
            {
                StartCoroutine(startTimer());
            }
        }
    }
}
