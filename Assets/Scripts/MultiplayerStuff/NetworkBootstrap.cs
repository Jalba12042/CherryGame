using Unity.Netcode;
using UnityEngine;

// NetworkBehaviours (like NetworkPlayerSpawner, for its ServerRpc) need to live on a spawned
// NetworkObject - they can't just sit on the NetworkManager GameObject itself, since that
// isn't a network object. This spawns a small dedicated one once the server starts.
public class NetworkBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject gameStatePrefab;

    // Start(), not Awake(): this sits on the same GameObject as NetworkManager itself, and
    // sibling components' Awake() order isn't guaranteed - NetworkManager.Singleton may not be
    // set yet if this runs first. Every object's Awake() finishes before any Start() does.
    private void Start()
    {
        Debug.Log($"[NetworkBootstrap] Start. gameStatePrefab assigned={gameStatePrefab != null}");
        NetworkManager.Singleton.OnServerStarted += SpawnGameState;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnServerStarted -= SpawnGameState;
    }

    private void SpawnGameState()
    {
        Debug.Log("[NetworkBootstrap] (Server) OnServerStarted fired, spawning NetworkGameState.");
        GameObject instance = Instantiate(gameStatePrefab);
        instance.GetComponent<NetworkObject>().Spawn();
    }
}
