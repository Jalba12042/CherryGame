using System.Collections;
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
    private bool roundStartRequested;

    private void Awake()
    {
        Instance = this;
        Debug.Log("[NetworkPlayerSpawner] Awake, Instance set.");
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[NetworkPlayerSpawner] OnNetworkSpawn. IsServer={IsServer} IsClient={IsClient} IsHost={IsHost}");
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
        ulong senderId = rpcParams.Receive.SenderClientId;
        registrations[senderId] = localPlayers;
        Debug.Log($"[NetworkPlayerSpawner] (Server) RegisterLocalPlayersServerRpc from client {senderId}: {localPlayers.Length} local player(s). Total registrations now: {registrations.Count}");
    }

    // Called by the host's own PlayerJoinController once it has registered its local players.
    // Doesn't load immediately: the host readying up is not the same as everyone else having
    // registered yet (that's a race - their RegisterLocalPlayersServerRpc might still be in
    // flight), so this waits until every currently-connected client has registered first.
    public void StartRound()
    {
        Debug.Log($"[NetworkPlayerSpawner] StartRound requested. IsServer={IsServer} registrations.Count={registrations.Count} connectedClients={(IsServer ? NetworkManager.ConnectedClientsIds.Count : -1)}");
        if (!IsServer || registrations.Count == 0 || roundStartRequested) return;
        roundStartRequested = true;
        StartCoroutine(WaitForAllRegistrationsThenLoad());
    }

    private IEnumerator WaitForAllRegistrationsThenLoad()
    {
        float waitTime = 0f;
        const float warnAfterSeconds = 5f;
        bool warned = false;

        while (registrations.Count < NetworkManager.ConnectedClientsIds.Count)
        {
            waitTime += Time.deltaTime;
            if (!warned && waitTime > warnAfterSeconds)
            {
                warned = true;
                Debug.LogWarning($"[NetworkPlayerSpawner] (Server) Still waiting for all clients to register after {warnAfterSeconds}s. " +
                    $"Registered so far: [{string.Join(", ", registrations.Keys)}]  Connected: [{string.Join(", ", NetworkManager.ConnectedClientsIds)}]");
            }
            yield return null;
        }

        Debug.Log($"[NetworkPlayerSpawner] (Server) All {registrations.Count} connected client(s) registered. Loading round scene '{roundSceneName}'.");
        spawnedThisRound = false;
        NetworkManager.SceneManager.LoadScene(roundSceneName, LoadSceneMode.Single);
    }

    private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        Debug.Log($"[NetworkPlayerSpawner] (Server) OnLoadEventCompleted for scene '{sceneName}'. clientsCompleted={clientsCompleted.Count} clientsTimedOut={clientsTimedOut.Count} spawnedThisRound={spawnedThisRound}");
        if (sceneName != roundSceneName || spawnedThisRound) return;
        spawnedThisRound = true;
        SpawnRegisteredPlayers();
    }

    private void SpawnRegisteredPlayers()
    {
        Debug.Log($"[NetworkPlayerSpawner] (Server) SpawnRegisteredPlayers: {registrations.Count} client(s) registered.");

        PlayerSpawn spawnPoints = FindFirstObjectByType<PlayerSpawn>();
        GameObject playerPrefab = NetworkManager.NetworkConfig.PlayerPrefab;
        if (spawnPoints == null || playerPrefab == null)
        {
            Debug.LogError($"[NetworkPlayerSpawner] Missing PlayerSpawn in scene (found={spawnPoints != null}) or PlayerPrefab on NetworkManager (found={playerPrefab != null}).");
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

                Debug.Log($"[NetworkPlayerSpawner] (Server) Spawned player globalIndex={globalIndex} for owner client {ownerClientId} at {pos}, NetworkObjectId={netObj.NetworkObjectId}");

                globalIndex++;
            }
        }

        Debug.Log($"[NetworkPlayerSpawner] (Server) Done spawning. Total players spawned: {globalIndex}");
        registrations.Clear();
    }
}
