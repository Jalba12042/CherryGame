using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PlayerSpawnManager : NetworkBehaviour
{
    [Header("Spawn Points")]
    public Transform hostSpawn;
    public Transform clientSpawn;

    [Header("Player Prefab")]
    public GameObject playerPrefab;

    public static PlayerSpawnManager Instance;

    private HashSet<ulong> spawnedPlayers = new HashSet<ulong>();

    private void Awake()
    {
        Instance = this;
        Debug.Log("[SpawnManager] Awake called. Singleton instance set.");
    }

    public (Vector3, Quaternion) GetSpawnTransform(ulong clientId)
    {
        Debug.Log($"[SpawnManager] Getting spawn transform for ClientID {clientId}.");

        // Safety checks
        if (hostSpawn == null || clientSpawn == null)
            Debug.LogWarning("[SpawnManager] One or more spawn points are not assigned!");

        if (NetworkManager.Singleton.IsHost && clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("[SpawnManager] Returning HOST spawn point.");
            return (hostSpawn != null ? hostSpawn.position : Vector3.zero,
                    hostSpawn != null ? hostSpawn.rotation : Quaternion.identity);
        }

        Debug.Log("[SpawnManager] Returning CLIENT spawn point.");
        return (clientSpawn != null ? clientSpawn.position : Vector3.zero,
                clientSpawn != null ? clientSpawn.rotation : Quaternion.identity);
    }

    // Call this when you're ready to actually spawn players (e.g. after pressing Play)
    [ServerRpc(RequireOwnership = false)]
    public void SpawnAllPlayersServerRpc()
    {
        Debug.Log("[SpawnManager] SpawnAllPlayersServerRpc called on SERVER.");

        if (playerPrefab == null)
        {
            Debug.LogError("[SpawnManager] Player prefab is missing! Cannot spawn players.");
            return;
        }

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[SpawnManager] NetworkManager.Singleton is null! Is the network running?");
            return;
        }

        var clients = NetworkManager.Singleton.ConnectedClientsList;
        Debug.Log($"[SpawnManager] Found {clients.Count} connected clients.");

        foreach (var client in clients)
        {
            ulong clientId = client.ClientId;
            Debug.Log($"[SpawnManager] Attempting to spawn player for ClientID {clientId}.");

            if (spawnedPlayers.Contains(clientId))
            {
                Debug.LogWarning($"[SpawnManager] ClientID {clientId} already has a spawned player. Skipping.");
                continue;
            }

            var (pos, rot) = GetSpawnTransform(clientId);
            Debug.Log($"[SpawnManager] Spawning player at position {pos}, rotation {rot}.");

            GameObject playerInstance = Instantiate(playerPrefab, pos, rot);
            var netObj = playerInstance.GetComponent<NetworkObject>();

            if (netObj == null)
            {
                Debug.LogError("[SpawnManager] Spawned prefab does not have a NetworkObject component! Destroying it.");
                Destroy(playerInstance);
                continue;
            }

            netObj.SpawnAsPlayerObject(clientId, true);
            spawnedPlayers.Add(clientId);

            Debug.Log($"[SpawnManager] Successfully spawned player for ClientID {clientId}.");
        }

        Debug.Log("[SpawnManager] All connected players processed for spawning.");
    }
}
