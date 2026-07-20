using Unity.Netcode;
using UnityEngine;

// Live-replicated customize-screen state: every claimed slot (local or remote, on any
// connected machine) shows up here so PlayerJoinController can render everyone's picks live,
// not just find out once the round starts. Lives on NetworkGameState alongside
// NetworkPlayerSpawner/OnlineRoundSync.
public class NetworkCustomizeSync : NetworkBehaviour
{
    public static NetworkCustomizeSync Instance;

    public const int MaxSlots = 4;

    public readonly NetworkList<PlayerSlotSyncData> Slots = new NetworkList<PlayerSlotSyncData>();

    private void Awake()
    {
        Instance = this;
        Debug.Log("[NetworkCustomizeSync] Awake, Instance set.");
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[NetworkCustomizeSync] OnNetworkSpawn. IsServer={IsServer}");
        if (IsServer)
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager != null)
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        if (Instance == this) Instance = null;
    }

    // Otherwise a disconnected player's preview model would linger on everyone else's screen.
    private void OnClientDisconnected(ulong clientId)
    {
        for (int i = Slots.Count - 1; i >= 0; i--)
        {
            if (Slots[i].ownerClientId == clientId)
            {
                Debug.Log($"[NetworkCustomizeSync] (Server) Client {clientId} disconnected, removing its slot (localSlotOnOwner={Slots[i].localSlotOnOwner}).");
                Slots.RemoveAt(i);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ClaimSlotServerRpc(int localSlotOnOwner, PlayerCustomizationData initialData, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        if (FindIndex(senderId, localSlotOnOwner) != -1) return; // already claimed

        if (Slots.Count >= MaxSlots)
        {
            Debug.LogWarning($"[NetworkCustomizeSync] (Server) ClaimSlotServerRpc from client {senderId} rejected - already at MaxSlots ({MaxSlots}).");
            return;
        }

        Slots.Add(new PlayerSlotSyncData
        {
            ownerClientId = senderId,
            localSlotOnOwner = localSlotOnOwner,
            data = initialData,
            ready = false
        });
        Debug.Log($"[NetworkCustomizeSync] (Server) Client {senderId} claimed slot (localSlotOnOwner={localSlotOnOwner}). Total slots: {Slots.Count}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateSlotDataServerRpc(int localSlotOnOwner, PlayerCustomizationData data, ServerRpcParams rpcParams = default)
    {
        int index = FindIndex(rpcParams.Receive.SenderClientId, localSlotOnOwner);
        if (index == -1) return;

        PlayerSlotSyncData entry = Slots[index];
        entry.data = data;
        Slots[index] = entry;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetSlotReadyServerRpc(int localSlotOnOwner, bool ready, ServerRpcParams rpcParams = default)
    {
        int index = FindIndex(rpcParams.Receive.SenderClientId, localSlotOnOwner);
        if (index == -1) return;

        PlayerSlotSyncData entry = Slots[index];
        entry.ready = ready;
        Slots[index] = entry;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReleaseSlotServerRpc(int localSlotOnOwner, ServerRpcParams rpcParams = default)
    {
        int index = FindIndex(rpcParams.Receive.SenderClientId, localSlotOnOwner);
        if (index == -1) return;

        Debug.Log($"[NetworkCustomizeSync] (Server) Client {rpcParams.Receive.SenderClientId} released slot (localSlotOnOwner={localSlotOnOwner}).");
        Slots.RemoveAt(index);
    }

    private int FindIndex(ulong ownerClientId, int localSlotOnOwner)
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i].ownerClientId == ownerClientId && Slots[i].localSlotOnOwner == localSlotOnOwner)
                return i;
        }
        return -1;
    }
}
