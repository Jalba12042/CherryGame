using UnityEngine;

// NEW: Forces Unity to add an AudioSource so you don't forget!
[RequireComponent(typeof(AudioSource))]
public class Crate : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip crashSound;
    private AudioSource audioSource;

    private bool hasSpawned = false;

    private void Awake()
    {
        // NEW: Grab the AudioSource
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (hasSpawned) return;
            else
            {
                hasSpawned = true;

                // NEW: Play the crash sound!
                if (crashSound != null)
                {
                    audioSource.PlayOneShot(crashSound);
                }

                GetComponent<MeshRenderer>().enabled = false;
                GetComponent<Collider>().enabled = false;

                RoundManager.Instance.powerupsInPlay.Add(Instantiate(RoundManager.Instance.powerUpsInRotation[Random.Range(0, RoundManager.Instance.powerUpsInRotation.Count)], transform.position, Quaternion.identity));

                // NEW: Wait for the sound to finish before destroying the object completely
                float destroyDelay = crashSound != null ? crashSound.length : 0f;
                Destroy(gameObject, destroyDelay);
            }
        }
    }
}
