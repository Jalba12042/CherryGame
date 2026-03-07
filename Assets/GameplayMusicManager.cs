using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class GameplayMusicManager : MonoBehaviour
{
    [Header("Music Tracks")]
    public AudioClip[] musicTracks; // Drag your gameplay songs here in the Inspector

    private AudioSource audioSource;
    private List<AudioClip> shuffledPlaylist = new List<AudioClip>();
    private int currentTrackIndex = 0;
    private bool isPlayingMusic = true;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        // Make sure looping is off so the script knows when the song ends
        audioSource.loop = false;
    }

    void Start()
    {
        if (musicTracks.Length > 0)
        {
            CreateShuffledPlaylist();
            PlayNextTrack();
        }
        else
        {
            Debug.LogWarning("No music tracks assigned to GameplayMusicManager!");
        }
    }

    void Update()
    {
        // If music should be playing, but the current track just finished...
        if (isPlayingMusic && !audioSource.isPlaying && musicTracks.Length > 0)
        {
            PlayNextTrack(); // ...play the next one!
        }
    }

    // This scrambles your songs into a random order
    void CreateShuffledPlaylist()
    {
        shuffledPlaylist.Clear();
        shuffledPlaylist.AddRange(musicTracks);

        // Standard shuffle algorithm (Fisher-Yates)
        for (int i = 0; i < shuffledPlaylist.Count; i++)
        {
            AudioClip temp = shuffledPlaylist[i];
            int randomIndex = Random.Range(i, shuffledPlaylist.Count);
            shuffledPlaylist[i] = shuffledPlaylist[randomIndex];
            shuffledPlaylist[randomIndex] = temp;
        }

        currentTrackIndex = 0;
    }

    void PlayNextTrack()
    {
        // If we reached the end of the shuffled list, reshuffle and start over
        if (currentTrackIndex >= shuffledPlaylist.Count)
        {
            CreateShuffledPlaylist();
        }

        audioSource.clip = shuffledPlaylist[currentTrackIndex];
        audioSource.Play();
        currentTrackIndex++;
    }

    // Call this method from your Timer script when the time is up!
    public void StopMusic()
    {
        isPlayingMusic = false;
        audioSource.Stop();
    }
}
