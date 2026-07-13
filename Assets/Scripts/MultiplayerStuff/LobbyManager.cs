using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The Multiplayer Widgets components (Create Session / Join Session By Code, wired to the
// project's WidgetConfiguration asset) already handle session creation, Relay allocation, and
// starting the NetworkManager as host or client. This script just watches for a successful
// connection and hands off to this machine's own local gamepad-assignment flow, which is
// unaffected by whether the session is local or online.
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

    private void Update()
    {
        bool connected = NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;
        if (continueButtonRoot != null) continueButtonRoot.SetActive(connected);
    }

    private void ContinueToLocalSetup()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
