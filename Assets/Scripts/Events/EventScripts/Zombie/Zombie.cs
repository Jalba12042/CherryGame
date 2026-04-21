using System.Collections;
using UnityEngine;

public enum ZombieState
{
    Wander,
    Rising,
    Chasing,
    Digging
}

[RequireComponent(typeof(AudioSource))]
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
    [SerializeField] private float spawnDepth = 2f;

    [Header("Digging")]
    [SerializeField] private float digTotalTime = 2f;

    [SerializeField] private GameObject dirtMoundPrefab;
    private GameObject spawnedDirt;
    private bool dirtFinished = false;

    [Header("Attacking")]
    [SerializeField] private GameObject hitbox;
    [SerializeField] private float attackMoveSpeed = 2.5f;
    [SerializeField] private Animator anim;
    [SerializeField] private float attackCooldown = 2.5f;
    public bool wasPlayer = false;

    [Header("Spawning")]
    public float spawnRadius;

    [Header("Audio")]
    public AudioClip riseSound;        // PLAYED AT START: Spawning / digging up
    public AudioClip attackSwingSound; // PLAYED ON ATTACK: Swinging / lunging
    public AudioClip despawnSound;     // PLAYED AT END: Digging back down
    public AudioClip[] moanSounds;     // PLAYED RANDOMLY: Groans while walking
    public float minMoanTime = 3f;
    public float maxMoanTime = 8f;

    private float moanTimer;
    private AudioSource audioSource;

    private Rigidbody rb;
    private Transform playerTarget;
    private float waitTimer;
    private float riseTimer;
    private float digTimer;
    private Vector3 wanderTarget;
    private bool isAttacking = false;
    private bool canAttack = true;
    private bool isInitialized = false;

    [Header("State")]
    public ZombieEvent myEvent;

    private void Awake()
    {
        hitbox.SetActive(false);

        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
            audioSource.playOnAwake = false;
    }

    private void Start()
    {
        // THIS IS THE FIX: Automatically trigger the intro dig & sound if it wasn't a dead player!
        if (!isInitialized && !wasPlayer)
        {
            InitNormalZombie();
        }
    }

    void ChangeState(ZombieState newState)
    {
        ZState = newState;
    }

    private IEnumerator LifeTimer()
    {
        // Wait until the event is officially over
        yield return new WaitUntil(() => !myEvent.isRunning);

        // --- OUTRO SOUND ---
        // Play the despawn digging sound right before they go underground!
        if (despawnSound != null)
        {
            Vector3 soundPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(despawnSound, soundPos, 1f);
        }

        digTimer = digTotalTime;
        ChangeState(ZombieState.Digging);
    }

    private void FixedUpdate()
    {
        if (!isInitialized)
            return;

        if (!RoundManager.Instance.currRoundActive) Destroy(gameObject);

        // --- RANDOM MOANING SOUNDS ---
        if ((ZState == ZombieState.Wander || ZState == ZombieState.Chasing) && moanSounds.Length > 0)
        {
            moanTimer -= Time.fixedDeltaTime;
            if (moanTimer <= 0f)
            {
                int randomMoan = Random.Range(0, moanSounds.Length);
                audioSource.PlayOneShot(moanSounds[randomMoan]);
                moanTimer = Random.Range(minMoanTime, maxMoanTime);
            }
        }

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
        if (!dirtFinished)
            return;

        if (rb.position.y < groundY)
        {
            float step = moveSpeed * Time.fixedDeltaTime;
            float newY = Mathf.Min(rb.position.y + step, groundY);
            rb.MovePosition(new Vector3(rb.position.x, newY, rb.position.z));
        }
        else
        {
            if (riseTimer <= 0f)
                riseTimer = riseWaitTime;

            riseTimer -= Time.fixedDeltaTime;

            if (riseTimer <= 0f)
            {
                wanderTarget = transform.position;
                rb.isKinematic = false;
                ChangeState(ZombieState.Wander);

                // start moaning after rising
                moanTimer = Random.Range(minMoanTime, maxMoanTime);

                StartCoroutine(LifeTimer());

                if (spawnedDirt != null)
                    Destroy(spawnedDirt);
            }
        }
    }

    public void SetDirtFinished()
    {
        dirtFinished = true;
    }

    void Dig()
    {
        hitbox.SetActive(false);
        anim.enabled = false;

        rb.isKinematic = true;

        float step = moveSpeed * Time.fixedDeltaTime;
        float newY = rb.position.y - step;
        rb.MovePosition(new Vector3(rb.position.x, newY, rb.position.z));

        digTimer -= Time.fixedDeltaTime;

        if (digTimer <= 0f)
            Destroy(gameObject);
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
            anim.SetBool("isMoving", false);
            waitTimer -= Time.fixedDeltaTime;

            if (waitTimer <= 0f)
                PickNewWanderPoint();
        }
    }

    void HandleChase()
    {
        if (playerTarget == null)
        {
            ChangeState(ZombieState.Wander);
            return;
        }

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
                anim.SetBool("isMoving", false);
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
            anim.SetBool("isMoving", true);

            Vector3 direction = offset / distance;
            float step = moveSpeed * Time.fixedDeltaTime;
            float clampedStep = Mathf.Min(step, distance);

            Vector3 newPosition = rb.position + direction * clampedStep;
            rb.MovePosition(newPosition);

            transform.forward = direction;
        }
        else
        {
            anim.SetBool("isMoving", false);
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
                offset.y = 0f;
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

        // --- ATTACK SOUND ---
        if (attackSwingSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSwingSound);
        }
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

    // THIS IS THE INTRO DIGGING LOGIC
    public void InitNormalZombie()
    {
        isInitialized = true;
        rb.isKinematic = true;

        if (dirtMoundPrefab != null)
        {
            Vector3 spawnPos = transform.position;
            spawnedDirt = Instantiate(dirtMoundPrefab, spawnPos, Quaternion.identity);

            DirtMound dirtScript = spawnedDirt.GetComponent<DirtMound>();
            if (dirtScript != null)
                dirtScript.Init(this);
        }

        Vector3 startPos = transform.position;
        startPos.y = groundY - spawnDepth;

        transform.position = startPos;
        rb.position = startPos;

        ChangeState(ZombieState.Rising);

        // --- INTRO SOUND ---
        if (riseSound != null)
            audioSource.PlayOneShot(riseSound);
    }

    public void InitAsPlayerZombie()
    {
        isInitialized = true;
        wasPlayer = true;
        rb.isKinematic = false;

        ChangeState(ZombieState.Wander);

        moanTimer = Random.Range(minMoanTime, maxMoanTime);
        StartCoroutine(LifeTimer());
    }
}