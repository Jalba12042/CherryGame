using System.Collections;
using UnityEngine;

public enum ZombieState {
    Wander,
    Rising,
    Chasing,
    Digging
}
public class Zombie : MonoBehaviour
{
    [SerializeField] private bool DEBUG_MODE = false;

    [Header("Zombie State")]
    [SerializeField] private ZombieState ZState;

    [Header("Movement")]
    [SerializeField] private float origMoveSpeed = 5f;
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
    [SerializeField] private float attackCooldown = 2.5f;
    public bool wasPlayer = false;

    [Header("Spawning")]
    public float spawnRadius;

    private Rigidbody rb;
    private Transform playerTarget;
    private float waitTimer;
    private float riseTimer;
    private float digTimer;
    private Vector3 wanderTarget;
    private bool isAttacking = false;
    private bool canAttack = true;

    [Header("State")]
    public ZombieEvent myEvent;

    private void Awake()
    {
        hitbox.SetActive(false);
        rb = GetComponent<Rigidbody>();
        
        if (!wasPlayer)
        {
            rb.isKinematic = true;
            ChangeState(ZombieState.Rising);
        }
        else
        {
            rb.isKinematic = false;
            ChangeState(ZombieState.Wander);
            StartCoroutine(LifeTimer());
        }
    }

    void ChangeState(ZombieState newState)
    {
        ZState = newState;
    }

    private IEnumerator LifeTimer()
    {
        yield return new WaitUntil(() => !myEvent.isRunning);
        //yield return new WaitForSeconds(10000);
        digTimer = digTotalTime;
        ChangeState(ZombieState.Digging);
    }

    private void FixedUpdate()
    {
        if (!RoundManager.Instance.currRoundActive) Destroy(gameObject);

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

    void Dig()
    {
        hitbox.SetActive(false);
        anim.enabled = false; // change this to start digging animation

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
            changeMoveSpeed(origMoveSpeed);
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
        // Guard: if target lost or dead, go back to wandering
        if (playerTarget == null)
        {
            ChangeState(ZombieState.Wander);
            return;
        }

        // Update position every frame from the live Transform
        Vector3 playerTargetPosition = playerTarget.position;

        Vector3 flatOffset = playerTargetPosition - rb.position;
        flatOffset.y = 0f;
        float distance = flatOffset.magnitude;

        if (distance > stoppingDist)
        {
            if (!isAttacking)
                changeMoveSpeed(origMoveSpeed);
        }
        else
        {
            if (!isAttacking && canAttack)
            {
                canAttack = false;
                isAttacking = true;
                anim.SetTrigger("attack");
                changeMoveSpeed(attackMoveSpeed);
            }
        }

        MoveTowards(playerTargetPosition);
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
                PlayerKill pk = hit.GetComponentInParent<PlayerKill>();
                if (pk != null && pk.currDead) continue;

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
                playerTarget = closest;
                ChangeState(ZombieState.Chasing);
            }
            else
            {
                playerTarget = null;
                ChangeState(ZombieState.Wander);
            }
        }
        else if (ZState == ZombieState.Chasing)
        {
            playerTarget = null;
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
        StartCoroutine(AttackCooldown());
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
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
