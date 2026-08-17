using System.Collections;
using UnityEngine;

public class GameplayMusicManager : MonoBehaviour
{
    public static GameplayMusicManager Instance;

    [Header("Music Tracks")]
    public AudioClip[] musicTracks;

    [Header("Fade Settings")]
    public float crossfadeDuration = 3f;
    public float maxVolume = 0.181f;

    private AudioSource audioSource;

    private void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();

        // Ensure it doesn't play immediately while the fake loading screen is up
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.volume = 0f;
        }
    }

    // Called by FakeLoadingScreen when the round officially begins
    public void StartMusic()
    {
        if (audioSource == null || musicTracks.Length == 0) return;

        // Pick a random track from the array
        int randomIndex = Random.Range(0, musicTracks.Length);
        audioSource.clip = musicTracks[randomIndex];

        audioSource.Play();
        StartCoroutine(FadeVolume(audioSource, maxVolume, crossfadeDuration));
    }

    // Called by EndOfRoundTransition when the timer hits zero
    public void StopMusicAndAmbience(AudioSource ambienceSource)
    {
        if (audioSource != null) StartCoroutine(FadeVolume(audioSource, 0f, 1.5f));
        if (ambienceSource != null) StartCoroutine(FadeVolume(ambienceSource, 0f, 1.5f));
    }

    // ==========================================
    // --- RESTORED FUNCTIONS TO FIX ERRORS ---
    // ==========================================

    // Called by WinScript to transition to the Shop
    public void FadeOutToShop(float duration)
    {
        if (audioSource != null)
        {
            StartCoroutine(FadeVolume(audioSource, 0f, duration));
        }
    }

    // Called by PlayerPowerupHandler when the Taco Dance starts!
    public void PauseGameplayMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    // Called by PlayerPowerupHandler when the Taco Dance ends!
    public void ResumeGameplayMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.UnPause();
        }
    }

    // ==========================================

    private IEnumerator FadeVolume(AudioSource source, float targetVol, float duration)
    {
        float startVol = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVol, targetVol, elapsed / duration);
            yield return null;
        }

        source.volume = targetVol;

        if (targetVol == 0f)
        {
            source.Stop();
        }
    }
}