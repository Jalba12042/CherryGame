using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameStarter : MonoBehaviour
{
    [Header("Scene To Load")]
    public string gameSceneName = "GameScene";

    private void Start()
    {
        // If NetworkManager isn't ready yet, wait for it
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[GameStarter] NetworkManager.Singleton is NULL. Make sure one exists in this scene.");
            return;
        }

        // If SceneManager isn't available yet, wait until network starts
        if (NetworkManager.Singleton.SceneManager == null)
        {
            Debug.Log("[GameStarter] SceneManager not yet initialized, waiting for network start...");
            NetworkManager.Singleton.OnServerStarted += RegisterSceneEvents;
            NetworkManager.Singleton.OnClientStarted += RegisterSceneEvents;
        }
        else
        {
            RegisterSceneEvents();
        }
    }

    private void RegisterSceneEvents()
    {
        if (NetworkManager.Singleton.SceneManager == null)
        {
            Debug.LogError("[GameStarter] Still no SceneManager found even after network start!");
            return;
        }

        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
        Debug.Log("[GameStarter] Successfully subscribed to SceneManager.OnLoadComplete!");
    }

    public void OnJoinGamePressed()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Only the host can start the game!");
            return;
        }

        Debug.Log("[GameStarter] Host is loading scene: " + gameSceneName);
        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (sceneName != gameSceneName)
            return;

        if (!NetworkManager.Singleton.IsHost)
            return;

        if (PlayerSpawnManager.Instance == null)
        {
            Debug.LogError("[GameStarter] No PlayerSpawnManager found in scene!");
            return;
        }

        Debug.Log($"[GameStarter] Scene '{sceneName}' loaded. Spawning players...");

        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var (pos, rot) = PlayerSpawnManager.Instance.GetSpawnTransform(id);
            GameObject player = Instantiate(NetworkManager.Singleton.NetworkConfig.PlayerPrefab, pos, rot);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
            Debug.Log($"[GameStarter] Spawned player for ClientID {id}.");
        }

        Debug.Log("[GameStarter] All players spawned successfully after scene load!");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
        }
    }
}
