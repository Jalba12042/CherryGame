using UnityEngine;
using System.Collections;

public class GunPowerup : Powerup
{
    [Header("Gun Settings")]
    public Transform barrel;
    public float range = 50f;
    public float fireRate = 0.5f;
    public LayerMask hitLayers;

    [Header("Attach Settings")]
    [Tooltip("Name of the child Transform on the player where the gun should attach (e.g. 'GunHand').")]
    public string handTransformName = "GunHand";
    public Vector3 localPositionOffset = Vector3.zero;
    public Vector3 localEulerOffset = Vector3.zero;
    public Vector3 heldScale = Vector3.one;

    private bool canShoot = true;
    private MeshRenderer[] meshRenderers;
    private Collider[] colliders;
    private Vector3 originalScale;

    private void Awake()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);
        originalScale = transform.localScale;
    }

    protected override void powerUpEffect()
    {
        base.powerUpEffect();

        if (powerupHandler == null) return;

        Transform hand = FindChildByName(powerupHandler.transform, handTransformName);
        if (hand == null)
        {
            Debug.LogWarning($"GunPowerup: could not find '{handTransformName}' under {powerupHandler.name}.");
            return;
        }

        transform.SetParent(hand, false);
        transform.localPosition = localPositionOffset;
        transform.localEulerAngles = localEulerOffset;
        transform.localScale = heldScale;

        SetVisible(true);
        SetCollidersEnabled(false);
        canShoot = true;
    }

    protected override void powerUpEnd()
    {
        StopAllCoroutines();
        canShoot = true;
        base.powerUpEnd();

        transform.SetParent(null, true);
        transform.localScale = originalScale;
        Destroy(gameObject);
    }

    private void Update()
    {
        if (powerupHandler == null) return;
        if (powerUpID < 0 || powerUpID >= powerupHandler.currPowerups.Count) return;
        if (!powerupHandler.currPowerups[powerUpID]) return;

        if (Input.GetMouseButton(0) && canShoot)
            StartCoroutine(Shoot());
    }

    IEnumerator Shoot()
    {
        canShoot = false;

        if (barrel != null)
        {
            Ray ray = new Ray(barrel.position, barrel.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range, hitLayers))
            {
                Debug.Log("Hit: " + hit.collider.name);
                PlayerKill pk = hit.collider.GetComponentInParent<PlayerKill>();
                if (pk != null) pk.killPlayer();
            }
            Debug.DrawRay(barrel.position, barrel.forward * range, Color.red, 0.2f);
        }

        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }

    private void SetVisible(bool visible)
    {
        foreach (var mr in meshRenderers)
            if (mr != null) mr.enabled = visible;
    }

    private void SetCollidersEnabled(bool enabled)
    {
        foreach (var c in colliders)
            if (c != null) c.enabled = enabled;
    }

    private static Transform FindChildByName(Transform root, string name)
    {
        if (root == null || string.IsNullOrEmpty(name)) return null;
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindChildByName(root.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }
}