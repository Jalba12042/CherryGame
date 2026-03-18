using UnityEngine;

public enum ZombieState {
    Wander,
    Rising,
    Following,
    Attacking,
    Digging
}
public class Zombie : MonoBehaviour
{
    [Header("Zombie State")]
    [SerializeField] private ZombieState ZState;

    [Header("Zombie Stats")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float stoppingDist = 0.2f;

    [Header("Wandering")]
    [SerializeField] private float wanderRadius = 5f; 
    [SerializeField] private float waitTimeAtPoint = 2f;

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

        Debug.Log(wanderTarget);
        switch (ZState)
        {
            case ZombieState.Wander:
                HandleWander();
                break;

            case ZombieState.Rising:
                
                break;

            case ZombieState.Following:

                break;

            case ZombieState.Attacking:
                
                break;

            case ZombieState.Digging:
                
                break;
        }
    }

    private void HandleWander()
    {
        Vector3 flatOffset = wanderTarget - rb.position;
        flatOffset.y = 0f;

        float distance = flatOffset.magnitude;

        if (distance > stoppingDist)
        {
            MoveTowards(wanderTarget);
            Debug.Log(distance);
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

    void ChangeState(ZombieState newState)
    {
        ZState = newState;
    }

    private void MoveTowards(Vector3 point)
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
}
