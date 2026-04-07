using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Scream : MonoBehaviour
{
    [SerializeField] private List<AudioClip> screamSFX;
    public float minPitch;
    public float maxPitch;
    
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

        if (player == null)
            player = GetComponentInParent<Playermovement>();

        if (faceAnimator == null)
            faceAnimator = GetComponent<Animator>();
    }


    private void Start()
    {
       // player = GetComponentInParent<Playermovement>();
        //aSource = GetComponent<AudioSource>();
        //aSource = GetComponentInParent<AudioSource>();
       // if (aSource == null)
       //     aSource = GetComponentInParent<AudioSource>();
        if (player != null)
            gp = player.assignedGamepad;
        //faceAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!GameManager.Instance.isOnKeyboard)
        {
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
        if (currentlyScreaming)
            yield break;

        currentlyScreaming = true;

        int rand = Random.Range(0, screamSFX.Count);
        float randPitch = Random.Range(minPitch, maxPitch);
        AudioClip clip = screamSFX[rand];

        aSource.pitch = randPitch;
        aSource.clip = clip;
        aSource.Play();

        // Turn ON the screaming animation state
        faceAnimator.SetBool("isScreaming", true);

        // wait for clip length adjusted by pitch
        yield return new WaitForSeconds(clip.length / Mathf.Abs(randPitch));

        // Turn OFF the screaming animation state
        faceAnimator.SetBool("isScreaming", false);

        currentlyScreaming = false;
    }
}
