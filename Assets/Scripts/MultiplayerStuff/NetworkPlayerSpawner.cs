using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// Server-authoritative spawner for the hybrid local co-op model: each connected client may
// bring 1-4 locally-controlled players (its own gamepads), all owned by that one connection.
// Lives on the persistent NetworkManager object, alongside NetworkManager itself.
public class NetworkPlayerSpawner : NetworkBehaviour
{
    public static NetworkPlayerSpawner Instance;

    [SerializeField] private string roundSceneName = "RossTestScene";

    private readonly Dictionary<ulong, PlayerCustomizationData[]> registrations = new Dictionary<ulong, PlayerCustomizationData[]>();
    private bool spawnedThisRound;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            NetworkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager != null && NetworkManager.SceneManager != null)
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterLocalPlayersServerRpc(PlayerCustomizationData[] localPlayers, ServerRpcParams rpcParams = default)
    {
        registrations[rpcParams.Receive.SenderClientId] = localPlayers;
    }

    // Called by the host's own PlayerJoinController once it has registered its local players.
    public void StartRound()
    {
        if (!IsServer || registrations.Count == 0) return;
        spawnedThisRound = false;
        NetworkManager.SceneManager.LoadScene(roundSceneName, LoadSceneMode.Single);
    }

    private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName != roundSceneName || spawnedThisRound) return;
        spawnedThisRound = true;
        SpawnRegisteredPlayers();
    }

    private void SpawnRegisteredPlayers()
    {
        PlayerSpawn spawnPoints = FindFirstObjectByType<PlayerSpawn>();
        GameObject playerPrefab = NetworkManager.NetworkConfig.PlayerPrefab;
        if (spawnPoints == null || playerPrefab == null)
        {
            Debug.LogError("[NetworkPlayerSpawner] Missing PlayerSpawn in scene or PlayerPrefab on NetworkManager.");
            registrations.Clear();
            return;
        }

        int globalIndex = 0;
        foreach (var kvp in registrations)
        {
            ulong ownerClientId = kvp.Key;
            PlayerCustomizationData[] localPlayers = kvp.Value;

            foreach (var customization in localPlayers)
            {
                if (globalIndex >= spawnPoints.spawnPoints.Length)
                {
                    Debug.LogWarning("[NetworkPlayerSpawner] More registered players than available spawn points; extra players not spawned.");
                    break;
                }

                Vector3 pos = spawnPoints.spawnPoints[globalIndex].position;
                GameObject instance = Instantiate(playerPrefab, pos, Quaternion.identity);

                Playermovement movement = instance.GetComponentInChildren<Playermovement>();
                if (movement != null) movement.GlobalIndex.Value = globalIndex;

                PlayerCustomization playerCustomization = instance.GetComponentInChildren<PlayerCustomization>();
                if (playerCustomization != null) playerCustomization.SetNetworkCustomization(customization);

                NetworkObject netObj = instance.GetComponent<NetworkObject>();
                netObj.SpawnWithOwnership(ownerClientId, true);

                globalIndex++;
            }
        }

        registrations.Clear();
    }
}
