using UnityEngine;
using System.Collections;

public class GunPowerup : Powerup
{
    [Header("Gun Settings")]
    public Transform barrel;          // where the ray starts
    public float range = 50f;
    public float fireRate = 0.5f;
    public LayerMask hitLayers;       // set to Player layer

    private bool canShoot = true;

    protected override void powerUpEffect()
    {
        base.powerUpEffect();
    }

    protected override void powerUpEnd()
    {
        base.powerUpEnd();
    }

    private void Update()
    {
        // Only allow shooting while this powerup is active
        if (!powerupHandler || !powerupHandler.currPowerups[powerUpID])
            return;

        // Replace this input with your control scheme if needed
        if (Input.GetMouseButton(0) && canShoot)
        {
            StartCoroutine(Shoot());
        }
    }

    IEnumerator Shoot()
    {
        canShoot = false;

        Ray ray = new Ray(barrel.position, barrel.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range, hitLayers))
        {
            Debug.Log("Hit: " + hit.collider.name);

            // Try to find a player
            PlayerKill pk = hit.collider.GetComponentInParent<PlayerKill>();

            if (pk != null)
            {
                pk.killPlayer();
            }
        }

        // Optional: debug line
        Debug.DrawRay(barrel.position, barrel.forward * range, Color.red, 0.2f);

        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }
}