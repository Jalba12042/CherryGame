using System.Collections;
using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    // This makes the script a "Singleton" so other scripts can talk to it easily
    public static MainMenuMusic Instance;

    public AudioSource audioSource;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    [Header("Gang Beasts Style Ducking")]
    [Tooltip("How much to lower the volume. 0.85 means it drops by 15%")]
    public float duckedVolumeMultiplier = 0.85f;
    [Tooltip("How fast the volume dips down when you press a button")]
    public float duckTransitionSpeed = 0.5f;

    private float startingVolume;
    private Coroutine volumeCoroutine;

    private void Awake()
    {
        // Check if a music manager already exists from a previous scene
        if (Instance == null)
        {
            Instance = this;
            // Tell Unity NOT to destroy this object when loading the lobby scenes!
            DontDestroyOnLoad(gameObject);

            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            startingVolume = audioSource.volume;
        }
        else
        {
            // If one already exists (like returning to the main menu), destroy this duplicate
            Destroy(gameObject);
        }
    }

    // Call this from your "Press Any Button" script!
    public void DuckMusicVolume()
    {
        if (volumeCoroutine != null) StopCoroutine(volumeCoroutine);
        volumeCoroutine = StartCoroutine(LerpVolume(startingVolume * duckedVolumeMultiplier, duckTransitionSpeed));
    }

    // Call this if you press Back and return to the Title Screen
    public void RestoreMusicVolume()
    {
        if (volumeCoroutine != null) StopCoroutine(volumeCoroutine);
        volumeCoroutine = StartCoroutine(LerpVolume(startingVolume, duckTransitionSpeed));
    }

    // Call this from your Loading Screen when the game is about to start
    public void FadeOutAndStop()
    {
        if (volumeCoroutine != null) StopCoroutine(volumeCoroutine);
        volumeCoroutine = StartCoroutine(LerpVolume(0f, fadeDuration));
    }

    private IEnumerator LerpVolume(float targetVolume, float duration)
    {
        float currentVol = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(currentVol, targetVolume, elapsed / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;

        // If the volume hit 0, stop the music completely and destroy the manager so it doesn't linger
        if (targetVolume == 0f)
        {
            audioSource.Stop();
            Destroy(gameObject);
        }
    }
}