using System.Collections;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    protected Playermovement pc;
    protected bool active = false; // is the powerup currently affecting the player?

    [SerializeField] protected float duration;
    [SerializeField] protected string puName;
    protected IEnumerator startTimer()
    {
        if (pc == null)
        {
            yield break;
        }

        // start powerup effects
        powerUpEffect();
        active = true;

        // make it look invisible - Victor: make the object drop out of the player's hand here as well
        GetComponent<Collider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;

        yield return new WaitForSeconds(duration);

        active = false;
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            pc = collision.gameObject.GetComponent<Playermovement>();
            StartCoroutine(startTimer());
        }    
    }

    protected virtual void powerUpEffect()
    {
        Debug.Log($"powerup: {puName}");
    }
}
