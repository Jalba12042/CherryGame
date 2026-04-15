using UnityEngine;

public class Meteor : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float extraGravity = 20f;
    private ScreenShake ss;
    private EnvironmentEffects ee;
    [SerializeField] private float moveForce;

    [SerializeField] private GameObject crackPrefab;
    [SerializeField] private LayerMask groundLayer;

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

        if (((1 << collision.gameObject.layer) & groundLayer) != 0 &&
        crackPrefab != null &&
        collision.contacts.Length > 0)
        {
            Vector3 hitPoint = collision.contacts[0].point;
            hitPoint.y = 32.44f;

            GameObject crack = Instantiate(crackPrefab, hitPoint, Quaternion.identity);

            SpawnCrack sc = crack.GetComponent<SpawnCrack>();
            if (sc != null)
            {
                sc.TriggerCrack();
            }
        }



        Destroy(gameObject);
    }
}
