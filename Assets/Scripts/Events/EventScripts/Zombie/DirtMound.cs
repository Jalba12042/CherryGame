using UnityEngine;

[RequireComponent(typeof(AudioSource))] // Automatically adds an Audio Source if you forget!
public class DirtMound : MonoBehaviour
{
    public float riseHeight = 1f;
    public float riseSpeed = 2f;

    private Vector3 startPos;
    private Vector3 targetPos;

    private Zombie zombie;

    private bool hasFinished = false; // NEW: Stops the finish logic from running multiple times

    public enum DirtMode
    {
        Rising,
        Sinking
    }

    private DirtMode mode;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip riseSound; // Sound for zombie appearing
    public AudioClip sinkSound; // Sound for zombie digging back down

    public void Init(Zombie z)
    {
        zombie = z;
        mode = DirtMode.Rising;
    }

    public void InitExit(Zombie z)
    {
        zombie = z;
        mode = DirtMode.Sinking;
    }

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        startPos = transform.position;

        if (mode == DirtMode.Rising)
        {
            targetPos = startPos + Vector3.up * riseHeight;

            // --- NEW: Play the rising dirt sound! ---
            if (riseSound != null)
            {
                audioSource.clip = riseSound;
                audioSource.Play();
            }
        }
        else
        {
            targetPos = startPos - Vector3.up * riseHeight;

            // --- NEW: Play the sinking dirt sound! ---
            if (sinkSound != null)
            {
                audioSource.clip = sinkSound;
                audioSource.Play();
            }
        }
    }

    void Update()
    {
        if (hasFinished) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            riseSpeed * Time.deltaTime
        );

        // When the dirt reaches its destination
        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            hasFinished = true;

            // --- NEW: Instantly cut the audio the second the dirt stops moving! ---
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            if (mode == DirtMode.Rising)
            {
                if (zombie != null)
                    zombie.SetDirtFinished();
            }
        }
    }
}