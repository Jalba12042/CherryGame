using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class JoinManager : MonoBehaviour
{
    [Header("Scene To Load When Game Starts")]
    public string gameSceneName = "GameScene"; // set this in the inspector

    // Called by the Join Game button (assign via the OnClick in inspector)
    public void OnJoinGamePressed()
    {
        // Only the host can trigger the scene change
        if (NetworkManager.Singleton.IsHost)
        {
            // Load the next scene for all clients
            NetworkManager.Singleton.SceneManager.LoadScene(
                gameSceneName,
                LoadSceneMode.Single
            );
        }
        else
        {
            Debug.Log("Only the host can start the game.");
        }
    }

    // Optional: if you want clients to connect via button
    public void OnHostPressed()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("Hosting started.");
    }

    public void OnClientPressed()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Client joining...");
    }
}
