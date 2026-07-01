using UnityEngine;

public class Snowball : LevelPickup
{
    private bool hasBeenThrown = false;
    [SerializeField] private float pushForce = 50f;

    protected override void Awake()
    {
        base.Awake();
        useProjectileThrow = false;
    }

    protected override void Update()
    {
        base.Update();
    }

    public void MarkThrown()
    {
        hasBeenThrown = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenThrown)
            return;

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