using UnityEngine;

public enum ZombieState {
    Wander,
    Rising,
    Chasing,
    Attacking,
    Digging
}
public class Zombie : MonoBehaviour
{
    [Header("Zombie State")]
    [SerializeField] private ZombieState ZState;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stoppingDist = 0.2f;

    [Header("Wandering")]
    [SerializeField] private float wanderRadius = 5f; 
    [SerializeField] private float waitTimeAtPoint = 2f;

    [Header("Chasing")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float detectionRange = 6f;

    [Header("Rising")]
    [SerializeField] private float risingDist;

    private Rigidbody rb;
    private Vector3 playerTarget;
    private float waitTimer;
    public Vector3 wanderTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        wanderTarget = transform.position;
        ChangeState(ZombieState.Wander);
    }
    private void FixedUpdate()
    {
        //if (!RoundManager.Instance.currRoundActive) Destroy(gameObject);

        switch (ZState)
        {
            case ZombieState.Wander:
                HandleWander();
                CheckForPlayer();
                break;

            case ZombieState.Rising:
                
                break;

            case ZombieState.Chasing:
                HandleChase();
                CheckForPlayer();
                break;

            case ZombieState.Attacking:
                
                break;

            case ZombieState.Digging:
                
                break;
        }
    }


    void HandleWander()
    {
        Vector3 flatOffset = wanderTarget - rb.position;
        flatOffset.y = 0f;

        float distance = flatOffset.magnitude;

        if (distance > stoppingDist)
        {
            MoveTowards(wanderTarget);
        }
        else
        {
            waitTimer -= Time.fixedDeltaTime;

            if (waitTimer <= 0f)
            {
                PickNewWanderPoint();
            }
        }
    }

    void PickNewWanderPoint()
    {
        Vector2 randPoint = Random.insideUnitCircle * wanderRadius;

        wanderTarget = new Vector3(
            rb.position.x + randPoint.x,
            rb.position.y,
            rb.position.z + randPoint.y
        );

        waitTimer = waitTimeAtPoint;
    }

    void HandleChase()
    {
        Vector3 flatOffset = playerTarget - rb.position;
        flatOffset.y = 0f;

        float distance = flatOffset.magnitude;

        if (distance > stoppingDist)
        {
            MoveTowards(playerTarget);
        }
        else
        {
            // attack
        }
    }

    void MoveTowards(Vector3 point)
    {
        Vector3 offset = point - rb.position;
        offset.y = 0f;

        float distance = offset.magnitude;

        if (distance > 0.001f)
        {
            Vector3 direction = offset / distance;

            float step = moveSpeed * Time.fixedDeltaTime;
            float clampedStep = Mathf.Min(step, distance);

            Vector3 newPosition = rb.position + direction * clampedStep;
            rb.MovePosition(newPosition);

            transform.forward = direction;
        }
    }

    private void CheckForPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);

        if (hits.Length > 0)
        {
            Transform closest = null;
            float closestDist = Mathf.Infinity;

            foreach (Collider hit in hits)
            {
                Vector3 offset = hit.transform.position - transform.position;
                offset.y = 0f; // ignore vertical difference
                float dist = offset.sqrMagnitude;

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = hit.transform;
                }
            }

            if (closest != null)
            {
                playerTarget = closest.position;
                ChangeState(ZombieState.Chasing);
            }
        }
        else if (ZState == ZombieState.Chasing)
        {
            ChangeState(ZombieState.Wander);
        }
    }

    void ChangeState(ZombieState newState)
    {
        ZState = newState;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
