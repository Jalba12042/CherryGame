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
    private Animator faceAnimator;
    private bool isScreaming = false;

    private void Start()
    {
        player = GetComponentInChildren<Playermovement>();
        aSource = GetComponent<AudioSource>();
        gp = player.assignedGamepad;
        faceAnimator = GetComponentInChildren<Animator>(); 

    }

    private void Update()
    {
        if (gp != null && gp.buttonEast.wasPressedThisFrame)
        {
            StartCoroutine(HandleScream());
        }
    }

    private IEnumerator HandleScream()
    {
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
        isScreaming = true;

        // Wait for scream duration
        yield return new WaitForSeconds(screamSFX[rand].length);

        // Reset face animation
        faceAnimator.SetBool("IsScreaming", false);
        isScreaming = false;
    }
}
