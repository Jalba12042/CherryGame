using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameStarter : MonoBehaviour
{
    [Header("Scene To Load")]
    public string gameSceneName = "GameScene";

    private void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[GameStarter] NetworkManager.Singleton is NULL. Make sure one exists in this scene.");
            return;
        }

        // Ensure SceneManager events are registered once the network is initialized
        if (NetworkManager.Singleton.SceneManager == null)
        {
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
            Debug.LogError("[GameStarter] SceneManager still missing after network start!");
            return;
        }

        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
        Debug.Log("[GameStarter] Registered OnLoadComplete event.");
    }

    public void OnJoinGamePressed()
    {
        // Only the host can initiate scene transitions
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.Log("[GameStarter] Only the host can start the game.");
            return;
        }

        Debug.Log($"[GameStarter] Host loading scene: {gameSceneName}");
        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        // Just log scene transitions — spawning handled elsewhere
        Debug.Log($"[GameStarter] Scene '{sceneName}' load complete for ClientID {clientId}.");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
        }
    }
}
