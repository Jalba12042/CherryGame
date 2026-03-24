using UnityEngine;
using System.Collections;

public class SprinklerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem sprinklerParticles;

    [Header("Movement Settings")]
    [SerializeField] private float riseHeight = 0.5f;   // How far it moves up
    [SerializeField] private float moveSpeed = 1.5f;    // Movement speed (m/s)

    [Header("Timing Settings")]
    [SerializeField] private Vector2 activeTimeRange = new Vector2(3f, 6f);  // How long it's active (random)
    [SerializeField] private Vector2 idleTimeRange = new Vector2(2f, 4f);    // How long it's off (random)
    [SerializeField] private bool autoStart = true;

    [SerializeField] private Collider waterTrigger;

    private Vector3 loweredPos;
    private Vector3 raisedPos;
    private bool isActive = false;
    private bool isMoving = false;
    private Coroutine sprinklerCycle;

    private void Start()
    {
        loweredPos = transform.position;
        raisedPos = loweredPos + Vector3.up * riseHeight;

        if (autoStart)
            StartSprinklerCycle();

        if (waterTrigger != null)
            waterTrigger.enabled = false;
    }

    private void Update()
    {
        if (isMoving)
            MoveSprinkler();
    }

    private void MoveSprinkler()
    {
        Vector3 target = isActive ? raisedPos : loweredPos;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        // Once reached destination
        if (Vector3.Distance(transform.position, target) < 0.001f)
        {
            isMoving = false;

            if (isActive)
            {
                if (!sprinklerParticles.isPlaying)
                {
                    sprinklerParticles.Play();

                    if (waterTrigger != null)
                        waterTrigger.enabled = true;
                }
            }
            else
            {
                if (sprinklerParticles.isPlaying)
                {
                    sprinklerParticles.Stop();

                    if (waterTrigger != null)
                        waterTrigger.enabled = false;
                }
            }
        }
    }

    // --- Public Controls ---

    [ContextMenu("Start Sprinkler Cycle")]
    public void StartSprinklerCycle()
    {
        if (sprinklerCycle != null)
            StopCoroutine(sprinklerCycle);

        sprinklerCycle = StartCoroutine(SprinklerRoutine());
    }

    [ContextMenu("Stop Sprinkler Cycle")]
    public void StopSprinklerCycle()
    {
        if (sprinklerCycle != null)
        {
            StopCoroutine(sprinklerCycle);
            sprinklerCycle = null;
        }
        DeactivateSprinkler();
    }

    private IEnumerator SprinklerRoutine()
    {
        while (true)
        {
            // Activate (rise + spray)
            ActivateSprinkler();
            float activeDuration = Random.Range(activeTimeRange.x, activeTimeRange.y);
            yield return new WaitForSeconds(activeDuration);

            // Deactivate (lower + stop)
            DeactivateSprinkler();
            float idleDuration = Random.Range(idleTimeRange.x, idleTimeRange.y);
            yield return new WaitForSeconds(idleDuration);
        }
    }

    private void ActivateSprinkler()
    {
        isActive = true;
        isMoving = true;
    }

    private void DeactivateSprinkler()
    {
        isActive = false;
        isMoving = true;
    }

    public bool IsActive()
    {
        return isActive;
    }

}
