using System.Collections;
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
    [SerializeField] private bool DEBUG_MODE = false;

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
    [SerializeField] private float groundY;
    [SerializeField] private float riseWaitTime = 1f;

    [Header("Digging")]
    [SerializeField] private float digTotalTime = 2f;

    [Header("Attacking")]
    [SerializeField] private GameObject hitbox;
    [SerializeField] private float attackMoveSpeed = 2.5f;
    [SerializeField] private Animator anim;

    private Rigidbody rb;
    private Vector3 playerTarget;
    private float waitTimer;
    private float riseTimer;
    private float digTimer;
    private Vector3 wanderTarget;
    private bool isAttacking = false;

    [Header("State")]
    public ZombieEvent myEvent;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        ChangeState(ZombieState.Rising);
    }

    void ChangeState(ZombieState newState)
    {
        ZState = newState;
    }

    private IEnumerator LifeTimer()
    {
        //yield return new WaitForSeconds(myEvent.duration);
        yield return new WaitForSeconds(10);
        digTimer = digTotalTime;
        ChangeState(ZombieState.Digging);
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
                Rise();
                break;

            case ZombieState.Chasing:
                HandleChase();
                CheckForPlayer();
                break;

            case ZombieState.Attacking:
                HandleAttack();
                break;

            case ZombieState.Digging:
                Dig();
                break;
        }
    }

    void Rise()
    {
        if (rb.position.y < groundY)
        {
            float step = moveSpeed * Time.fixedDeltaTime;
            float newY = Mathf.Min(rb.position.y + step, groundY);
            rb.MovePosition(new Vector3(rb.position.x, newY, rb.position.z));
        }
        else
        {
            if (riseTimer <= 0f)
            {
                riseTimer = riseWaitTime; 
            }

            riseTimer -= Time.fixedDeltaTime;

            if (riseTimer <= 0f)
            {
                wanderTarget = transform.position;
                rb.isKinematic = false;
                ChangeState(ZombieState.Wander);
                StartCoroutine(LifeTimer());
            }
        }
    }

    void HandleAttack()
    {

    }

    void Dig()
    {
        rb.isKinematic = true;

        float step = moveSpeed * Time.fixedDeltaTime;
        float newY = rb.position.y - step;
        rb.MovePosition(new Vector3(rb.position.x, newY, rb.position.z));

        digTimer -= Time.fixedDeltaTime;

        if (digTimer <= 0f)
        {
            Destroy(gameObject);
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

    void HandleChase()
    {
        Vector3 flatOffset = playerTarget - rb.position;
        flatOffset.y = 0f;

        float distance = flatOffset.magnitude;

        if (distance > stoppingDist)
        {
            if (!isAttacking) 
                changeMoveSpeed(moveSpeed);
        }
        else
        {
            if (!isAttacking)
            {
                isAttacking = true;
                anim.SetTrigger("attack");
                changeMoveSpeed(attackMoveSpeed);
            }
        }

        MoveTowards(playerTarget);
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

    void changeMoveSpeed(float newMoveSpeed)
    {
        moveSpeed = newMoveSpeed;
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

    public void StartAttack()
    {
        hitbox.SetActive(true);
    }

    public void EndAttack()
    {
        hitbox.SetActive(false);
        isAttacking = false;
    }

    private void OnDrawGizmos()
    {
        if (DEBUG_MODE)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}
