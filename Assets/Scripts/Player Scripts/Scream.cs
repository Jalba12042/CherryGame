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
        
        gp = player.assignedGamepad;
        //faceAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!GameManager.Instance.isOnKeyboard)
        {
            if (gp != null && gp.buttonEast.wasPressedThisFrame)
            {
                StartCoroutine(HandleScream());
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                StartCoroutine(HandleScream());
            }
        }
    }

    private IEnumerator HandleScream()
    {
        faceAnimator.SetBool("IsScreaming", false);
        // Stop any ongoing scream sound
        aSource.Stop();

        // Pick random scream clip and pitch
        int rand = Random.Range(0, screamSFX.Count);
        float randPitch = Random.Range(minPitch, maxPitch);
        aSource.pitch = randPitch;

        // Play scream
        aSource.PlayOneShot(screamSFX[rand]);

        // Trigger face animation
        faceAnimator.SetBool("IsScreaming", true);

        // Wait for scream duration
        yield return new WaitForSeconds(screamSFX[rand].length);

        // Reset face animation
        faceAnimator.SetBool("IsScreaming", false);
    }
}
