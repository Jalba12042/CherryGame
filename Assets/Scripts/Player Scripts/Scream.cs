using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Scream : MonoBehaviour
{
    [SerializeField] private List<AudioClip> screamSFX;
    
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
            aSource.Stop();
            int rand = Random.Range(0, screamSFX.Count);
            float randPitch = Random.Range(0f, 2f);
            aSource.pitch = randPitch;
            aSource.PlayOneShot(screamSFX[rand]);
        }
    }
}
