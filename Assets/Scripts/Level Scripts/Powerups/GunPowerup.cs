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

        GameObject bullet = Instantiate(bulletPrefab, barrel.position, barrel.rotation);

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

        Destroy(gameObject);

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