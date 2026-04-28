using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Networking.Transport.Relay;

public class MultiplayerLobbyManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text joinCodeDisplay;
    public TMP_InputField joinCodeInput;
    public Button startGameButton;
    public TMP_Text statusText;

    async void Start()
    {
        // Initialize Unity Gaming Services
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        // Hide start button until a client connects
        startGameButton.gameObject.SetActive(false);
        SetStatus("Choose Create or Join");
    }

    // Wire this to your "Create Game" button
    public async void OnHostClicked()
    {
        SetStatus("Creating session...");
        try
        {
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(4);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);

            NetworkManager.Singleton.GetComponent<UnityTransport>()
            .SetHostRelayData(
                alloc.RelayServer.IpV4,
                (ushort)alloc.RelayServer.Port,
                alloc.AllocationIdBytes,
                alloc.Key,
                alloc.ConnectionData
               );

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.StartHost();

            joinCodeDisplay.text = "Code: " + joinCode;
            SetStatus("Waiting for players to join...");
        }
        catch (System.Exception e)
        {
            SetStatus("Error: " + e.Message);
        }
    }

    // Wire this to your "Join Game" button
    public async void OnJoinClicked()
    {
        string code = joinCodeInput.text.Trim().ToUpper();
        if (string.IsNullOrEmpty(code)) return;

        SetStatus("Connecting...");
        try
        {
            JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(code);

            NetworkManager.Singleton.GetComponent<UnityTransport>()
            .SetClientRelayData(
                joinAlloc.RelayServer.IpV4,
                (ushort)joinAlloc.RelayServer.Port,
                joinAlloc.AllocationIdBytes,
                joinAlloc.Key,
                joinAlloc.ConnectionData,
                joinAlloc.HostConnectionData
    );

            NetworkManager.Singleton.StartClient();
            SetStatus("Connected! Waiting for host to start...");
        }
        catch (System.Exception e)
        {
            SetStatus("Failed to join: " + e.Message);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Show start button once at least one client is connected
        if (NetworkManager.Singleton.IsHost)
            startGameButton.gameObject.SetActive(true);
    }

    // Wire this to your "Start Game" button
    public void OnStartGameClicked()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        // Loads CharacterSelect on every connected client
        NetworkManager.Singleton.SceneManager.LoadScene(
            "ControllerConnectScene",  // replace with your exact scene name
            UnityEngine.SceneManagement.LoadSceneMode.Single
        );
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }
}