using UnityEngine;

public class AmbienceController : MonoBehaviour
{
    private AudioSource audioSource;
    private bool wasPlayingBeforePause = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (audioSource == null) return;

        // If time is frozen (game is paused) and the audio is currently playing, pause it.
        if (Time.timeScale == 0f && audioSource.isPlaying)
        {
            audioSource.Pause();
            wasPlayingBeforePause = true;
        }
        // If time is running normally and the audio was paused by this script, resume it.
        else if (Time.timeScale > 0f && wasPlayingBeforePause && !audioSource.isPlaying)
        {
            audioSource.UnPause();
            wasPlayingBeforePause = false;
        }
    }
}