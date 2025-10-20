using UnityEngine;

public class Powerup : MonoBehaviour
{
    protected PlayerController pc;
    protected virtual void powerUpEffect()
    {
        Debug.Log("powerup");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            pc = collision.gameObject.GetComponent<PlayerController>();
            powerUpEffect();
            Destroy(gameObject);
        }
            
    }
}
