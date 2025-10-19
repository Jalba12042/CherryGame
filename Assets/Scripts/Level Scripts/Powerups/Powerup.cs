using UnityEngine;

public class Powerup : MonoBehaviour
{
    protected PlayerMovement pm;
    protected virtual void powerUpEffect()
    {
        Debug.Log("powerup");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            pm = collision.gameObject.GetComponent<PlayerMovement>();
            powerUpEffect();
            Destroy(gameObject);
        }
            
    }
}
