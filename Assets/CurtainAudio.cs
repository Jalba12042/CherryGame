using UnityEngine;

public class CurtainAudio : MonoBehaviour
{
    [Header("Audio Setup")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    // --- We leave this here just to catch that annoying "ghost" marker 
    // so Unity NEVER throws that red error at you again! ---
    public void PlaySound()
    {
        // Do nothing, just eat the error.
    }

    // --- The actual triggers ---
    public void PlayOpen()
    {
        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }
    }

    public void PlayClose()
    {
        if (audioSource != null && closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }
    }
}