using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using System.Collections;

public class LevelPickup : NetworkBehaviour
{
    [HideInInspector] public bool isHeld = false;
    [HideInInspector] public GameObject playerHolding;

    [Header("Trail")]
    [SerializeField] private GameObject trailObject;

    public bool useProjectileThrow = false;

    private GroundCheck groundCheck;
    private bool wasGrounded = false;

    [HideInInspector] public bool ignoreBasketPull = false;

    public int pointValue = 1;

    // Server-authoritative hold state, replicated so every peer mirrors the cosmetic
    // parent-to-hand visual. The server keeps sole physics authority - carrying never
    // transfers NetworkObject ownership to the holding player.
    private readonly NetworkVariable<ulong> holderNetworkObjectId = new NetworkVariable<ulong>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Rigidbody rb;
    private NetworkTransform networkTransform;

    public IEnumerator TemporarilyIgnoreBasket(float duration)
    {
        ignoreBasketPull = true;
        yield return new WaitForSeconds(duration);
        ignoreBasketPull = false;
    }

    protected virtual void Awake()
    {
        groundCheck = GetComponent<GroundCheck>();
        rb = GetComponent<Rigidbody>();
        networkTransform = GetComponent<NetworkTransform>();
        if (trailObject != null) trailObject.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        holderNetworkObjectId.OnValueChanged += (_, current) => ApplyHolder(current);
        ApplyHolder(holderNetworkObjectId.Value);
    }

    protected virtual void Update()
    {
        if (groundCheck == null) return;
        if (groundCheck.isGrounded && !wasGrounded)
            DisableTrail();
        wasGrounded = groundCheck.isGrounded;
    }

    public void EnableTrail()
    {
        if (trailObject != null) trailObject.SetActive(true);
    }

    public void DisableTrail()
    {
        if (trailObject != null) trailObject.SetActive(false);
    }

    // ===== Online pickup/drop - mirrors what PlayerInteract does directly in local play,
    // but routed through the server since it alone may write the networked transform. =====

    [ServerRpc(RequireOwnership = false)]
    public void RequestPickupServerRpc(ulong holderPlayerObjectId)
    {
        if (holderNetworkObjectId.Value != 0) return; // already held by someone
        holderNetworkObjectId.Value = holderPlayerObjectId;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestDropServerRpc()
    {
        holderNetworkObjectId.Value = 0;
    }

    private void ApplyHolder(ulong holderId)
    {
        isHeld = holderId != 0;

        if (holderId != 0 && NetworkManager.Singleton != null && NetworkManager.Singleton.SpawnManager != null &&
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(holderId, out NetworkObject holderObj))
        {
            playerHolding = holderObj.gameObject;
            PlayerInteract interact = holderObj.GetComponentInChildren<PlayerInteract>();
            Transform hand = interact != null ? interact.handHoldPoint : null;
            if (hand != null)
            {
                transform.SetParent(hand);
                transform.localPosition = Vector3.zero;
            }
        }
        else
        {
            playerHolding = null;
            if (transform.parent != null) transform.SetParent(null);
        }

        if (rb != null) rb.isKinematic = isHeld;
        if (networkTransform != null) networkTransform.enabled = !isHeld;
    }
}
