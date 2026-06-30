using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Meteor : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float extraGravity = 20f;
    private ScreenShake ss;
    private EnvironmentEffects ee;
    [SerializeField] private float moveForce;

    [SerializeField] private GameObject crackPrefab;
    [SerializeField] private LayerMask groundLayer;

    [Header("Audio Settings")]
    [Tooltip("The sound that loops while the meteor is falling.")]
    public AudioClip fallingSound;

    [Tooltip("The sound that plays on impact.")]
    public AudioClip crashSound;

    [Range(0f, 1f)] public float volume = 1f;

    private AudioSource audioSource;

    private bool exploded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ss = FindFirstObjectByType<ScreenShake>();
        ee = FindAnyObjectByType<EnvironmentEffects>();

        // Setup the AudioSource for the falling loop
        audioSource = GetComponent<AudioSource>();
        if (fallingSound != null)
        {
            audioSource.clip = fallingSound;
            audioSource.loop = true;
            audioSource.spatialBlend = 0f; // Force 2D so it's audible from space
            audioSource.volume = volume;
            audioSource.Play();
        }
    }

    void FixedUpdate()
    {
        rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (exploded)
            return;

        exploded = true;

        // SOUND FIX: Play at Camera position so the player always hears the impact
        if (crashSound != null)
        {
            Vector3 soundPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(crashSound, soundPos, volume);
        }

        // Check for player kill
        PlayerKill pk = collision.gameObject.GetComponentInChildren<PlayerKill>();
        if (pk != null)
        {
            pk.killPlayer(true);
        }

        // Visual and Physical effects
        ss?.Shake();
        ee?.bigImpact(moveForce, null);

        // Ground Crack Logic
        if (((1 << collision.gameObject.layer) & groundLayer) != 0 &&
            crackPrefab != null &&
            collision.contacts.Length > 0)
        {
            Vector3 hitPoint = collision.contacts[0].point;

            // Adjust this Y value to match your ground height
            hitPoint.y = 32.44f;

            GameObject crack = Instantiate(crackPrefab, hitPoint, Quaternion.identity);

            SpawnCrack sc = crack.GetComponent<SpawnCrack>();
            if (sc != null)
            {
                sc.TriggerCrack();
            }
        }

        // Destroy the meteor
        Destroy(gameObject);
    }
}