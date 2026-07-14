using UnityEngine;
using System.Collections;

public class GunPowerup : Powerup
{
    [Header("Gun Settings")]
    public Transform barrel;          // where the ray starts
    public float range = 50f;
    public float fireRate = 0.5f;
    public LayerMask hitLayers;       // set to Player layer

    private bool hasShot = false;

    private Animator playerAnimator;

    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 25f;

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

    // called by player when RT pressed first time
    public void EquipGun(Transform hand)
    {
        // get ROOT animator only
        playerAnimator = hand.root.GetComponent<Animator>();

        if (playerAnimator != null)
            playerAnimator.SetBool("isPickingUp", true);

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

    // called by player when RT pressed second time
    public void Fire()
    {
        if (hasShot) return;
        StartCoroutine(Shoot());
    }

    IEnumerator Shoot()
    {
        hasShot = true;

        // --- 1. PLAY SHOOT SOUND ---
        float destroyDelay = 0f;
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
            destroyDelay = shootSound.length;
        }

        GameObject bullet = Instantiate(bulletPrefab, barrel.position, barrel.rotation);

        // --- NEW: TELL THE BULLET WHO SHOT IT ---
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null && powerupHandler != null)
        {
            Playermovement pm = powerupHandler.GetComponent<Playermovement>();
            if (pm != null)
            {
                bulletScript.shooterID = pm.playerIndex; // Pass the ID to the bullet!
            }
        }

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = barrel.forward * bulletSpeed;

        Destroy(bullet, 5f);

        if (playerAnimator != null)
            playerAnimator.SetBool("isPickingUp", false);

        transform.SetParent(null);

        // HARD CLEAN (IMPORTANT ORDER)
        if (powerupHandler != null)
        {
            powerupHandler.hasGunEquipped = false;
            powerupHandler.activeGun = null;
        }

        powerUpEnd();

        // --- 2. HIDE THE GUN INSTANTLY ---
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }

        // --- 3. DESTROY AFTER SOUND FINISHES ---
        Destroy(gameObject, destroyDelay);

        yield break;
    }

    protected override void powerUpEnd()
    {
        base.powerUpEnd();

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isPickingUp", false);
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
            playerAnimator.SetBool("isPickingUp", false);
        }
    }
}