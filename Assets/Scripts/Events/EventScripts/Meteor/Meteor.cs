using UnityEngine;

public class Meteor : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float extraGravity = 20f;
    private ScreenShake ss;
    private EnvironmentEffects ee;
    [SerializeField] private float moveForce;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ss = FindFirstObjectByType<ScreenShake>();
        ee = FindAnyObjectByType<EnvironmentEffects>();
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

        ss?.Shake();
        ee?.bigImpact(moveForce, null);
        // add explosion effect here
        Destroy(gameObject);
    }
}
