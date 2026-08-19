using UnityEngine;

public class Snowball : LevelPickup
{
    private bool hasBeenThrown = false;
    [SerializeField] private float pushForce = 50f;

    private GameObject owner;

    [Header("Audio Settings")]
    [Tooltip("Add your 2 random hit sounds here!")]
    public AudioClip[] impactSounds;
    [Range(0f, 1f)] public float impactVolume = 1f;

    protected override void Awake()
    {
        base.Awake();
        useProjectileThrow = false;
    }

    protected override void Update()
    {
        base.Update();
    }

    public void SetOwner(GameObject player)
    {
        owner = player;
    }

    public void MarkThrown()
    {
        hasBeenThrown = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenThrown)
            return;

        if (collision.gameObject == owner || collision.transform.root.gameObject == owner)
        {
            return;
        }

        PlayerPowerupHandler handler =
            collision.gameObject.GetComponentInParent<PlayerPowerupHandler>();

        if (handler != null)
        {
            Playermovement movement =
    collision.gameObject.GetComponentInParent<Playermovement>();

            if (movement != null)
            {
                // --- NEW: Play a random impact sound before the snowball is destroyed! ---
                PlayRandomImpactSound();

                Vector3 pushDirection = collision.transform.position - transform.position;
                pushDirection.y = 0f;
                pushDirection.Normalize();

                movement.ApplyKnockback(pushDirection, 40f, 0.25f);

                Destroy(gameObject);
                return;
            }
        }

        Destroy(gameObject);
    }

    private void PlayRandomImpactSound()
    {
        // Make sure we actually have sounds loaded in the array
        if (impactSounds != null && impactSounds.Length > 0)
        {
            // Pick a random number between 0 and the amount of sounds you added
            int randomIndex = Random.Range(0, impactSounds.Length);
            AudioClip clipToPlay = impactSounds[randomIndex];

            if (clipToPlay != null)
            {
                // This spawns a temporary audio player at the hit location that deletes itself when the sound finishes!
                AudioSource.PlayClipAtPoint(clipToPlay, transform.position, impactVolume);
            }
        }
    }
}