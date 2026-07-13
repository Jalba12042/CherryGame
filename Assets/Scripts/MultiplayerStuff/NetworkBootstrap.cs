using Unity.Netcode;
using UnityEngine;

// NetworkBehaviours (like NetworkPlayerSpawner, for its ServerRpc) need to live on a spawned
// NetworkObject - they can't just sit on the NetworkManager GameObject itself, since that
// isn't a network object. This spawns a small dedicated one once the server starts.
public class NetworkBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject gameStatePrefab;

    private void Awake()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnGameState;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnServerStarted -= SpawnGameState;
    }

    private void SpawnGameState()
    {
        GameObject instance = Instantiate(gameStatePrefab);
        instance.GetComponent<NetworkObject>().Spawn();
    }
}
