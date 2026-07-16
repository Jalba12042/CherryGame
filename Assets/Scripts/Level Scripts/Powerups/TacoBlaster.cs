using UnityEngine;
using System.Collections;

public class TacoBlaster : Powerup
{
    [Header("Taco Blaster Settings")]
    public Transform barrel;
    public float fireRate = 0.5f;

    private bool hasShot = false;
    private Animator playerAnimator;

    [Header("Projectile")]
    [SerializeField] private GameObject tacoProjectilePrefab;
    [SerializeField] private float projectileSpeed = 25f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shootSound;

    private void Awake()
    {
        if (!isHoldable)
            despawnRoutine = StartCoroutine(despawnTimer());
    }

    protected override void powerUpEffect()
    {
        base.powerUpEffect();
        hasShot = false;

        if (activeTimer != null)
        {
            StopCoroutine(activeTimer);
            activeTimer = null;
        }
    }

    public void EquipTacoBlaster(Transform hand)
    {
        playerAnimator = hand.root.GetComponent<Animator>();

        if (playerAnimator != null)
            playerAnimator.SetBool("isHoldingTB", true);

        transform.SetParent(hand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        GetComponent<PowerUpFloat>()?.SetHeld(true);
    }

    public void Fire()
    {
        if (hasShot) return;

        StartCoroutine(Shoot());
    }

    IEnumerator Shoot()
    {
        hasShot = true;

        if (playerAnimator != null)
            playerAnimator.SetTrigger("shootTB");

        float destroyDelay = 0f;

        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
            destroyDelay = shootSound.length;
        }

        GameObject projectile =
            Instantiate(tacoProjectilePrefab, barrel.position, barrel.rotation);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = barrel.forward * projectileSpeed;
        }

        Destroy(projectile, 5f);

        if (playerAnimator != null)
            playerAnimator.SetBool("isHoldingTB", false);

        transform.SetParent(null);

        if (powerupHandler != null)
        {
            powerupHandler.hasTacoBlasterEquipped = false;
            powerupHandler.activeTacoBlaster = null;
        }

        powerUpEnd();

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }

        Destroy(gameObject, destroyDelay);

        yield break;
    }

    protected override void powerUpEnd()
    {
        base.powerUpEnd();

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isHoldingTB", false);
        }
    }

    protected override IEnumerator StartTimer()
    {
        while (true)
        {
            yield return null;
        }
    }

    private void OnDestroy()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isHoldingTB", false);
        }
    }
}