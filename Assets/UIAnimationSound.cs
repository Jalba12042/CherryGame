using UnityEngine;

public class UIAnimationSound : MonoBehaviour
{
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // We will trigger this from the Animation Timeline!
    public void PlaySound()
    {
        if (audioSource != null) audioSource.Play();
    }
}