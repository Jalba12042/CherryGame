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



    protected override void powerUpEffect()
    {
        base.powerUpEffect();
        hasShot = false;
    }

    // called by player when RT pressed first time
    public void EquipGun(Transform hand)
    {
        transform.SetParent(hand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
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

        Ray ray = new Ray(barrel.position, barrel.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range, hitLayers))
        {
            PlayerKill pk = hit.collider.GetComponentInParent<PlayerKill>();
            if (pk != null)
                pk.killPlayer();
        }

        Debug.DrawRay(barrel.position, barrel.forward * range, Color.red, 0.2f);

        yield return new WaitForSeconds(fireRate);

        // consume powerup
        powerUpEnd();
        Destroy(gameObject);
    }

    protected override void powerUpEnd()
    {
        base.powerUpEnd();
    }
}