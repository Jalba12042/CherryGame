using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// --- NEW: Custom box to hold the name, clip, and custom volume! ---
[System.Serializable]
public class ScreamAudio
{
    public string screamName; // e.g. "Max Loud", "Ross Quiet"
    public AudioClip clip;
    [Range(0.1f, 3f)] public float volumeBooster = 1.0f; // Slide up to make louder, down to make quieter
}

public class Scream : MonoBehaviour
{
    // --- NEW: Uses the custom list instead of the basic AudioClip list ---
    [SerializeField] private List<ScreamAudio> screamSFX;

    [Header("Audio Settings")]
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;
    [Range(0f, 1f)] public float screamVolume = 1f; // The "Master" volume

    private Playermovement player;
    public AudioSource aSource;
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

        // Foolproof check. If it STILL can't find one, make one!
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

    private void Update()
    {
        if (player == null) return;

        // Do not allow screaming while the pause menu or resume countdown is active
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;

        if (InputManager.Instance.GetScreamDown(player.playerID))
            TryScream();
    }

    // Unity kills any running coroutine when this GameObject is deactivated (e.g. visualRoot
    // turned off on death) without letting HandleScream() reach its cleanup at the end, so
    // currentlyScreaming would otherwise get stuck true and block every scream afterward.
    private void OnDisable()
    {
        if (screamRoutine != null)
        {
            StopCoroutine(screamRoutine);
            screamRoutine = null;
        }

        currentlyScreaming = false;

        if (faceAnimator != null) faceAnimator.SetBool("isScreaming", false);
        if (aSource != null) aSource.Stop();
    }

    private IEnumerator HandleScream()
    {
        if (currentlyScreaming || screamSFX.Count == 0)
            yield break;

        currentlyScreaming = true;

        // 1. Pick a random scream from our new custom list
        int rand = Random.Range(0, screamSFX.Count);
        ScreamAudio selectedScream = screamSFX[rand];

        float randPitch = Random.Range(minPitch, maxPitch);

        aSource.pitch = randPitch;

        // 2. THE MAGIC TRICK: Master Volume multiplied by this specific clip's booster!
        aSource.volume = screamVolume * selectedScream.volumeBooster;

        aSource.clip = selectedScream.clip;
        aSource.Play();

        // Turn ON the screaming animation state
        if (faceAnimator != null) faceAnimator.SetBool("isScreaming", true);

        // wait for clip length adjusted by pitch
        yield return new WaitForSeconds(selectedScream.clip.length / Mathf.Abs(randPitch));

        // Turn OFF the screaming animation state
        if (faceAnimator != null) faceAnimator.SetBool("isScreaming", false);

        currentlyScreaming = false;
    }
}