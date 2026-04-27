using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))] // Automatically adds an Audio Source if you forget!
public class CherryBomb : Cherry
{
    private bool fuseLit = false;
    [SerializeField] private float fuseTime;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionForce;
    [SerializeField] private float upwardModifier = 1.5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Visuals")]
    [SerializeField] private GameObject fuseVFX;
    [SerializeField] private Renderer cherryRenderer;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material flashingMaterial;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fuseTickingSound;
    [SerializeField] private AudioClip explosionSound;

    [Header("Explosion VFX")]
    [SerializeField] private GameObject explosionVFXPrefab;

    private void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (fuseVFX != null)
            fuseVFX.SetActive(false);

        if (cherryRenderer != null && normalMaterial != null)
            cherryRenderer.material = normalMaterial;
    }

    private void Update()
    {
        if (isHeld)
        {
            if (fuseVFX != null)
                fuseVFX.SetActive(true);

            if (cherryRenderer != null && flashingMaterial != null)
                SetCherryMaterial(flashingMaterial);
        }

        if (isHeld && !fuseLit)
        {
            fuseLit = true;

            // --- NEW: Play the ticking sound the exact frame the fuse is lit! ---
            if (fuseTickingSound != null)
            {
                audioSource.clip = fuseTickingSound;
                audioSource.loop = true; // Loops the ticking until it explodes
                audioSource.Play();
            }

            StartCoroutine(LightFuse());
        }
    }

    private IEnumerator LightFuse()
    {
        Debug.Log("Fuse lit! Ticking down...");
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    private void Explode()
    {
        if (explosionVFXPrefab != null)
        {
            GameObject vfx = Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);

            // scale to match explosion radius
            float diameter = explosionRadius * 2f;
            vfx.transform.localScale = Vector3.one * diameter;

            // destroy after a short time (VFX Graph doesn't auto-destroy)
            Destroy(vfx, 2f);
        }

        // 1. Do the physics push and kill the players
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider h in hits)
        {
            if ((groundLayer & (1 << h.gameObject.layer)) != 0)
                continue; // skip ground objects

            Rigidbody rb = h.GetComponent<Rigidbody>();
            if (rb == null)
                continue;

            rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardModifier, ForceMode.Impulse);

            PlayerKill pk = rb.GetComponent<PlayerKill>();
            if (pk != null)
                pk.killPlayer();
        }

        // 2. Play Explosion Sound (and stop the ticking!)
        float destroyDelay = 0.1f; // Fallback delay
        if (explosionSound != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(explosionSound);
            destroyDelay = explosionSound.length; // Set delay to exactly how long the boom is
        }

        // 3. Make the bomb INVISIBLE and UNTOUCHABLE while the sound finishes playing
        if (cherryRenderer != null) cherryRenderer.enabled = false;
        if (fuseVFX != null) fuseVFX.SetActive(false);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 4. Finally, destroy the object after the sound is totally done
        Destroy(gameObject, destroyDelay);
    }

    void SetCherryMaterial(Material mat)
    {
        Material[] mats = cherryRenderer.materials;
        mats[0] = mat; // ONLY swap main cherry material
        cherryRenderer.materials = mats;
    }
}