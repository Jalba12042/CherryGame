using System.Collections;
using UnityEngine;

public enum UFOState { Approaching, Hovering, Abducting, Leaving }

[RequireComponent(typeof(AudioSource))]
public class UFO : MonoBehaviour
{
    public GameObject[] players;
    public GameObject targetPlayer;
    public Playermovement playerMove;
    public PlayerKill playerKill;

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
    public AudioClip hoverSound;
    public AudioClip abductSound;
    private AudioSource audioSource; // NEW

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>(); // NEW

        players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) return;

        int randPlayer = Random.Range(0, players.Length);
        targetPlayer = players[randPlayer];
        playerMove = targetPlayer.GetComponentInChildren<Playermovement>();
        playerKill = targetPlayer.GetComponentInChildren<PlayerKill>();

        ChangeState(UFOState.Approaching);
    }

    private void Update()
    {
        if (targetPlayer == null) return;
        if (!RoundManager.Instance.currRoundActive) Destroy(gameObject);

        if (ufoModel != null)
        {
            ufoModel.Rotate(Vector3.up * spinSpeed * Time.deltaTime);
        }

        switch (currentState)
        {
            case UFOState.Approaching: HandleApproach(); break;
            case UFOState.Hovering: HandleHover(); break;
            case UFOState.Abducting: HandleAbduct(); break;
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
        targetPlayer.transform.position += liftDirection * abductSpeed * Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, targetPlayer.transform.position + Vector3.up * hoverHeight, 5f * Time.deltaTime);

        stateTimer += Time.deltaTime;

        if (stateTimer > 5f)
        {
            myEvent.isRunning = false;
            playerKill.killPlayer();
            Destroy(gameObject);
        }
    }

    private void ChangeState(UFOState nextState)
    {
        currentState = nextState;
        stateTimer = 0f;

        // NEW: Audio Logic for State Changes
        if (currentState == UFOState.Approaching)
        {
            audioSource.clip = hoverSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        else if (currentState == UFOState.Abducting)
        {
            audioSource.Stop(); // Stop the hover hum
            audioSource.PlayOneShot(abductSound); // Play the abduction beam
        }
    }
}