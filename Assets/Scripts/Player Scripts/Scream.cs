using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Scream : MonoBehaviour
{
    [SerializeField] private List<AudioClip> screamSFX;

    [Header("Audio Settings")]
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;
    [Range(0f, 1f)] public float screamVolume = 1f; // NEW: Volume slider so you can make it loud!

    private Playermovement player;
    public AudioSource aSource;
    private Gamepad gp;
    [SerializeField] private Animator faceAnimator;

    private Coroutine screamRoutine;
    private bool currentlyScreaming;

    private void TryScream()
    {
        // 1. If we are already screaming, ignore the button press entirely.
        if (currentlyScreaming)
            return;

        // 2. Start the scream coroutine
        screamRoutine = StartCoroutine(HandleScream());
    }

    private void Awake()
    {
        if (aSource == null)
            aSource = GetComponentInChildren<AudioSource>() ?? GetComponentInParent<AudioSource>();

        // NEW: Foolproof check. If it STILL can't find one, make one!
        if (aSource == null)
        {
            aSource = gameObject.AddComponent<AudioSource>();
            aSource.playOnAwake = false;
        }

        if (player == null)
            player = GetComponentInParent<Playermovement>();

        if (faceAnimator == null)
            faceAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (player != null)
        {
            gp = player.assignedGamepad;
        }
    }

    private void Update()
    {
        if (!GameManager.Instance.isOnKeyboard)
        {
            // buttonEast is the 'B' button on Xbox / Circle on PlayStation!
            if (gp != null && gp.buttonEast.wasPressedThisFrame)
            {
                TryScream();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                TryScream();
            }
        }
    }

    private IEnumerator HandleScream()
    {
        if (currentlyScreaming || screamSFX.Count == 0)
            yield break;

        currentlyScreaming = true;

        int rand = Random.Range(0, screamSFX.Count);
        float randPitch = Random.Range(minPitch, maxPitch);
        AudioClip clip = screamSFX[rand];

        aSource.pitch = randPitch;
        aSource.volume = screamVolume; // NEW: Sets it to your loud volume
        aSource.clip = clip;
        aSource.Play();

        // Turn ON the screaming animation state
        if (faceAnimator != null) faceAnimator.SetBool("isScreaming", true);

        // wait for clip length adjusted by pitch
        yield return new WaitForSeconds(clip.length / Mathf.Abs(randPitch));

        // Turn OFF the screaming animation state
        if (faceAnimator != null) faceAnimator.SetBool("isScreaming", false);

        currentlyScreaming = false;
    }
}