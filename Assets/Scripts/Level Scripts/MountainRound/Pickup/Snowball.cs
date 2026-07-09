using UnityEngine;

public class Snowball : LevelPickup
{
    private bool hasBeenThrown = false;
    [SerializeField] private float pushForce = 50f;

    private GameObject owner;

    protected override void Awake()
    {
        base.Awake();
        useProjectileThrow = false;
    }

    protected override void Update()
    {
        base.Update();
    }

    public void SetOwner(GameObject player)
    {
        owner = player;
    }

    public void MarkThrown()
    {
        hasBeenThrown = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenThrown)
            return;

        if (collision.gameObject == owner || collision.transform.root.gameObject == owner)
        {
            return;
        }

        PlayerPowerupHandler handler =
            collision.gameObject.GetComponentInParent<PlayerPowerupHandler>();

        if (handler != null)
        {
            Playermovement movement =
    collision.gameObject.GetComponentInParent<Playermovement>();

            if (movement != null)
            {
                Vector3 pushDirection = collision.transform.position - transform.position;
                pushDirection.y = 0f;
                pushDirection.Normalize();

                movement.ApplyKnockback(pushDirection, 40f, 0.25f);

                Destroy(gameObject);
                return;
            }
        }

        Destroy(gameObject);
    }
}