using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UFOState { Approaching, Hovering, Abducting, Leaving, Waiting }

[RequireComponent(typeof(AudioSource))]
public class UFO : MonoBehaviour
{
    public GameObject[] players;
    public GameObject targetPlayer;
    public Playermovement playerMove;
    public PlayerKill playerKill;
    private Rigidbody targetRb;

    [Header("Touch Transfer")]
    [SerializeField] private float touchTransferRadius = 1.5f;
    [SerializeField] private float touchTransferCooldown = 1.5f;
    private float touchCooldownUntil = 0f;

    private UFOState currentState;

    [Header("UFO Stats")]
    [SerializeField] private float hoverHeight;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float hoverDuration;
    [SerializeField] private float abductSpeed;

    [SerializeField] private Transform ufoModel;
    [SerializeField] private float spinSpeed = 50f;
    private float stateTimer;

    public UFOEvent myEvent;

    [Header("Audio")]
    public AudioClip spawnIntroSound; // NEW: Sound when it first appears
    public AudioClip hoverSound;
    public AudioClip abductSound;
    private AudioSource audioSource;

    [Header("Beam")]
    [SerializeField] private Transform beam; // assign your beam object
    [SerializeField] private float beamFlashStartInterval = 0.5f;  // blink speed right as hovering begins
    [SerializeField] private float beamFlashEndInterval = 0.05f;   // blink speed right before abduction starts
    private Coroutine beamFlashRoutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Play the spawn sound immediately!
        if (spawnIntroSound != null)
        {
            audioSource.PlayOneShot(spawnIntroSound);
        }

        players = GameObject.FindGameObjectsWithTag("Player");

        if (!TryRetargetToLivingPlayer())
        {
            ChangeState(UFOState.Waiting);
        }
    }

    private void Update()
    {
        if (!RoundManager.Instance.currRoundActive)
        {
            Destroy(gameObject);
            return;
        }

        if (ufoModel != null)
        {
            ufoModel.Rotate(Vector3.up * spinSpeed * Time.deltaTime);
        }

        // No one alive to abduct right now — just sit tight and keep checking
        if (currentState == UFOState.Waiting)
        {
            TryRetargetToLivingPlayer();
            return;
        }

        if (targetPlayer == null) return;

        if (playerKill != null && playerKill.currDead)
        {
            if (!TryRetargetToLivingPlayer())
            {
                ChangeState(UFOState.Waiting);
                return;
            }
        }

        CheckForTouchTransfer();

        switch (currentState)
        {
            case UFOState.Approaching: HandleApproach(); break;
            case UFOState.Hovering: HandleHover(); break;
            case UFOState.Abducting: HandleAbduct(); break;
        }
    }

    // Picks another living player to chase (used after the current target dies, or when nobody was alive yet)
    private bool TryRetargetToLivingPlayer()
    {
        List<GameObject> alivePlayers = new List<GameObject>();
        foreach (GameObject p in players)
        {
            if (p == null) continue;
            PlayerKill pk = p.GetComponentInChildren<PlayerKill>();
            if (pk != null && !pk.currDead) alivePlayers.Add(p);
        }

        if (alivePlayers.Count == 0) return false;

        SetTarget(alivePlayers[Random.Range(0, alivePlayers.Count)]);
        return true;
    }

    // A touch hands off tracking without resetting stateTimer, so the current target can't stall
    // the abduction forever by staying near a crowd — proximity is symmetric either way.
    private void CheckForTouchTransfer()
    {
        if (targetPlayer == null) return;

        // Without this, a transfer to the closest player immediately qualifies as a transfer back
        // (distance is symmetric), so two players standing near each other cause the target to
        // flip-flop every single frame instead of settling on one handoff.
        if (Time.time < touchCooldownUntil) return;

        float closestDist = float.MaxValue;
        GameObject closest = null;

        foreach (GameObject p in players)
        {
            if (p == null || p == targetPlayer) continue;

            PlayerKill pk = p.GetComponentInChildren<PlayerKill>();
            if (pk == null || pk.currDead) continue;

            float dist = Vector3.Distance(targetPlayer.transform.position, p.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = p;
            }
        }

        if (closest != null && closestDist <= touchTransferRadius)
        {
            SetTarget(closest, resetState: false);
            touchCooldownUntil = Time.time + touchTransferCooldown;
        }
    }

    // resetState true = a fresh abduction attempt (initial spawn / target died) — restarts the whole
    // approach sequence. resetState false = a touch mid-flight just hands tracking to someone else
    // without touching stateTimer, so the abduction can't be stalled by repeatedly getting touched.
    private void SetTarget(GameObject newTarget, bool resetState = true)
    {
        // Release whoever we were carrying/tracking so a mid-lift retarget doesn't leave them frozen
        if (playerMove != null) playerMove.canMove = true;
        if (targetRb != null) targetRb.isKinematic = false;

        targetPlayer = newTarget;
        playerMove = targetPlayer.GetComponentInChildren<Playermovement>();
        playerKill = targetPlayer.GetComponentInChildren<PlayerKill>();
        targetRb = targetPlayer.GetComponentInChildren<Rigidbody>();

        if (resetState)
        {
            ChangeState(UFOState.Approaching);
        }
        else if (currentState == UFOState.Abducting && targetRb != null)
        {
            // Still mid-lift after the handoff — make sure the new target gets the same
            // kinematic treatment the old one had, instead of staying fully dynamic.
            targetRb.isKinematic = true;
        }
    }

    private void HandleApproach()
    {
        Vector3 targetPosition = targetPlayer.transform.position + Vector3.up * hoverHeight;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            ChangeState(UFOState.Hovering);
        }
    }

    private void HandleHover()
    {
        stateTimer += Time.deltaTime;
        Vector3 targetPosition = targetPlayer.transform.position + Vector3.up * hoverHeight;
        transform.position = Vector3.Lerp(transform.position, targetPosition, 5f * Time.deltaTime);

        if (stateTimer >= hoverDuration)
        {
            ChangeState(UFOState.Abducting);
        }
    }

    private void HandleAbduct()
    {
        Vector3 liftDirection = Vector3.up;
        playerMove.canMove = false;

        // Move via the (now-kinematic) Rigidbody instead of teleporting transform.position directly —
        // a plain transform write on a physics body can tunnel through other players' colliders or
        // fight the Rigidbody's own physics step, making the "bump into them to free them" touch
        // detection register inconsistently.
        if (targetRb != null)
            targetRb.MovePosition(targetRb.position + liftDirection * abductSpeed * Time.deltaTime);
        else
            targetPlayer.transform.position += liftDirection * abductSpeed * Time.deltaTime;

        transform.position = Vector3.Lerp(transform.position, targetPlayer.transform.position + Vector3.up * hoverHeight, 5f * Time.deltaTime);

        stateTimer += Time.deltaTime;

        if (stateTimer > 3f)
        {
            playerKill.killPlayer(myEvent.respawnOnKill);
            myEvent.isRunning = false;
            Destroy(gameObject);
        }
    }

    private void ChangeState(UFOState nextState)
    {
        currentState = nextState;
        stateTimer = 0f;

        // Any state change (including dropping into Waiting) cancels a running flash so it can't
        // keep blinking in the background or double up next time we start hovering again.
        if (beamFlashRoutine != null)
        {
            StopCoroutine(beamFlashRoutine);
            beamFlashRoutine = null;
        }

        if (beam != null)
        {
            if (currentState == UFOState.Hovering)
            {
                beam.gameObject.SetActive(false); // first flash tick turns it on
                beamFlashRoutine = StartCoroutine(FlashBeamRoutine());
            }
            else
            {
                beam.gameObject.SetActive(currentState == UFOState.Abducting);
            }
        }

        if (currentState == UFOState.Approaching)
        {
            audioSource.clip = hoverSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        else if (currentState == UFOState.Abducting)
        {
            // Kinematic while lifted so it stops fighting gravity/physics each frame, but still
            // reports collisions correctly against the (non-kinematic) player who bumps into them.
            if (targetRb != null) targetRb.isKinematic = true;

            audioSource.Stop();
            audioSource.PlayOneShot(abductSound);
        }
    }

    // Blinks the beam faster and faster as stateTimer closes in on hoverDuration, so the flash
    // rate always tracks however close we actually are to the abduction starting.
    private IEnumerator FlashBeamRoutine()
    {
        while (currentState == UFOState.Hovering)
        {
            float progress = hoverDuration > 0f ? Mathf.Clamp01(stateTimer / hoverDuration) : 1f;
            float interval = Mathf.Lerp(beamFlashStartInterval, beamFlashEndInterval, progress);

            beam.gameObject.SetActive(!beam.gameObject.activeSelf);
            yield return new WaitForSeconds(interval);
        }

        beamFlashRoutine = null;
    }
}