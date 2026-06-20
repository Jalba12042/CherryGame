using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class GameplayMusicManager : MonoBehaviour
{
    public static GameplayMusicManager Instance; // Makes this easy to call from WinScript!

    [Header("Music Tracks")]
    public AudioClip[] musicTracks;

    [Header("Fade Settings")]
    public float crossfadeDuration = 3f; // How long tracks blend into each other
    public float maxVolume = 0.181f;     // The normal volume for your music

    private AudioSource[] audioSources;
    private int activeSourceIndex = 0;

    private List<AudioClip> shuffledPlaylist = new List<AudioClip>();
    private int currentTrackIndex = 0;
    private bool isPlayingMusic = true;
    private bool isCrossfading = false;

    private float pausedVolume0;
    private float pausedVolume1;

    void Awake()
    {
        // 1. Keep the music alive when the Win Scene loads!
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 2. Set up the Double Audio Source trick
        audioSources = new AudioSource[2];
        audioSources[0] = GetComponent<AudioSource>(); // The one you already have
        audioSources[1] = gameObject.AddComponent<AudioSource>(); // The secret backup one!

        // Make sure the backup source has the exact same Audio Mixer settings
        audioSources[1].outputAudioMixerGroup = audioSources[0].outputAudioMixerGroup;

        audioSources[0].loop = false;
        audioSources[1].loop = false;
        audioSources[1].volume = 0f; // Start silent
    }

    void Start()
    {
        if (musicTracks.Length > 0)
        {
            CreateShuffledPlaylist();
            PlayNextTrack(true); // True means start instantly, no fade
        }
        else
        {
            Debug.LogWarning("No music tracks assigned!");
        }
    }

    void Update()
    {
        if (!isPlayingMusic) return;

        AudioSource activeSource = audioSources[activeSourceIndex];

        // 3. Start the crossfade right before the current song ends!
        if (activeSource.isPlaying && !isCrossfading)
        {
            float timeRemaining = activeSource.clip.length - activeSource.time;
            if (timeRemaining <= crossfadeDuration)
            {
                PlayNextTrack(false); // False means do a smooth crossfade
            }
        }
    }

    void CreateShuffledPlaylist()
    {
        shuffledPlaylist.Clear();
        shuffledPlaylist.AddRange(musicTracks);

        for (int i = 0; i < shuffledPlaylist.Count; i++)
        {
            AudioClip temp = shuffledPlaylist[i];
            int randomIndex = Random.Range(i, shuffledPlaylist.Count);
            shuffledPlaylist[i] = shuffledPlaylist[randomIndex];
            shuffledPlaylist[randomIndex] = temp;
        }

        currentTrackIndex = 0;
    }

    void PlayNextTrack(bool instantStart)
    {
        if (currentTrackIndex >= shuffledPlaylist.Count)
        {
            CreateShuffledPlaylist();
        }

        int nextSourceIndex = 1 - activeSourceIndex; // Swaps between 0 and 1
        AudioSource nextSource = audioSources[nextSourceIndex];
        AudioSource currentSource = audioSources[activeSourceIndex];

        nextSource.clip = shuffledPlaylist[currentTrackIndex];
        currentTrackIndex++;

        if (instantStart)
        {
            nextSource.volume = maxVolume;
            nextSource.Play();
            activeSourceIndex = nextSourceIndex;
        }
        else
        {
            StartCoroutine(Crossfade(currentSource, nextSource, crossfadeDuration));
        }
    }

    IEnumerator Crossfade(AudioSource fadeOutSource, AudioSource fadeInSource, float duration)
    {
        isCrossfading = true;
        fadeInSource.Play();
        fadeInSource.volume = 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Blend the volumes
            fadeOutSource.volume = Mathf.Lerp(maxVolume, 0f, t);
            fadeInSource.volume = Mathf.Lerp(0f, maxVolume, t);

            yield return null;
        }

        fadeOutSource.volume = 0f;
        fadeOutSource.Stop();
        fadeInSource.volume = maxVolume;

        activeSourceIndex = System.Array.IndexOf(audioSources, fadeInSource);
        isCrossfading = false;
    }

    // --- NEW: Triggered when everyone hits A in the Win Scene! ---
    public void FadeOutToShop(float duration)
    {
        if (!isPlayingMusic) return;
        StartCoroutine(FadeOutAndDestroy(duration));
    }

    IEnumerator FadeOutAndDestroy(float duration)
    {
        isPlayingMusic = false;
        float elapsed = 0f;

        float startVol0 = audioSources[0].volume;
        float startVol1 = audioSources[1].volume;

        // Fade both sources out over time
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            audioSources[0].volume = Mathf.Lerp(startVol0, 0f, t);
            audioSources[1].volume = Mathf.Lerp(startVol1, 0f, t);
            yield return null;
        }

        audioSources[0].Stop();
        audioSources[1].Stop();

        // Destroy this object completely so it doesn't overlap when the next round starts!
        Destroy(gameObject);
    }

    public void PauseGameplayMusic()
    {
        pausedVolume0 = audioSources[0].volume;
        pausedVolume1 = audioSources[1].volume;

        audioSources[0].volume = 0f;
        audioSources[1].volume = 0f;
    }

    public void ResumeGameplayMusic()
    {
        audioSources[0].volume = pausedVolume0;
        audioSources[1].volume = pausedVolume1;
    }
}