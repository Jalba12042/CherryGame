using UnityEngine;

public class Meteor : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float extraGravity = 20f;
    private ScreenShake ss;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ss = FindFirstObjectByType<ScreenShake>();
    }

    void FixedUpdate()
    {
        rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerKill pk = collision.gameObject.GetComponentInChildren<PlayerKill>();
        if (pk != null)
        {
            pk.killPlayer();
        }

        ss.Shake();
        // add explosion effect here
        Destroy(gameObject);
    }
}
