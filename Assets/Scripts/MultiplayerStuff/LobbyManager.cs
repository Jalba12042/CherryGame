using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The Multiplayer Widgets components (Create Session / Join Session By Code, wired to the
// project's WidgetConfiguration asset) already handle session creation, Relay allocation, and
// starting the NetworkManager as host or client. This script watches for a successful
// connection and lets the HOST pull everyone into the local gamepad-assignment flow together
// via a networked scene load - non-host clients don't get their own button, they just wait to
// be carried along, so joining the lobby before the host is ready doesn't leave them behind.
public class LobbyManager : MonoBehaviour
{
    [SerializeField] private GameObject continueButtonRoot;
    [SerializeField] private Button continueButton;
    [SerializeField] private string nextSceneName = "ControllerConnectScene";

    private void Awake()
    {
        if (continueButtonRoot != null) continueButtonRoot.SetActive(false);
        if (continueButton != null) continueButton.onClick.AddListener(ContinueToLocalSetup);
    }

    private bool loggedConnected;

    private void Update()
    {
        bool connected = NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;
        if (connected && !loggedConnected)
        {
            loggedConnected = true;
            Debug.Log($"[LobbyManager] Connected. IsHost={NetworkManager.Singleton.IsHost} IsServer={NetworkManager.Singleton.IsServer} IsClient={NetworkManager.Singleton.IsClient} LocalClientId={NetworkManager.Singleton.LocalClientId}");
        }

        // Only the host can trigger the networked scene load below, so only the host gets a
        // button - other clients just wait to be pulled along with everyone else.
        bool showButton = connected && NetworkManager.Singleton.IsHost;
        if (continueButtonRoot != null) continueButtonRoot.SetActive(showButton);
    }

    private void ContinueToLocalSetup()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsHost) return;
        Debug.Log($"[LobbyManager] Host pressed Continue, loading '{nextSceneName}' for all connected clients.");
        NetworkManager.Singleton.SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }
}
