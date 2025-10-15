using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Scream : MonoBehaviour
{
    [SerializeField] private List<AudioClip> screamSFX;
    [SerializeField] private float minPitch;
    [SerializeField] private float maxPitch;
    
    private PlayerMovement player;
    private AudioSource aSource;
    private Gamepad gp;

    private void Start()
    {
        player = GetComponent<PlayerMovement>();
        aSource = GetComponent<AudioSource>();
        gp = player.assignedGamepad;
    }

    private void Update()
    {
        if (gp.buttonEast.wasPressedThisFrame)
        {
            // stop any screams
            aSource.Stop();

            // pick a random pitch for the scream
            int rand = Random.Range(0, screamSFX.Count);
            float randPitch = Random.Range(minPitch, maxPitch);
            aSource.pitch = randPitch;
            aSource.PlayOneShot(screamSFX[rand]);
        }
    }
}
