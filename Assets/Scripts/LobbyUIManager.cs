using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class LobbyUIManager : MonoBehaviour
{
    [Header("UI")]
    public Button createLobbyButton;
    public Button refreshLobbyButton;
    public Transform lobbyListParent;
    public GameObject lobbyButtonPrefab;

    private void Awake()
    {
        if (createLobbyButton != null)
            createLobbyButton.onClick.AddListener(async () => await CreateLobby());

        if (refreshLobbyButton != null)
            refreshLobbyButton.onClick.AddListener(async () => await RefreshLobbyList());
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async Task CreateLobby()
    {
        try
        {
            // 1) Create Relay allocation (host)
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // 2) Create Lobby and store join code in lobby data
            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                }
            };

            await LobbyService.Instance.CreateLobbyAsync("MyLobby", 4, options);

            // 3) Configure UnityTransport using explicit fields from allocation
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("UnityTransport not found on NetworkManager.");
                return;
            }

            // NOTE: this is the direct SetRelayServerData call that matches Unity 6 style.
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.ConnectionData // host uses its connectionData for hostConnectionData as well
            );

            // 4) Start host
            NetworkManager.Singleton.StartHost();

            Debug.Log("Lobby created. Join code: " + joinCode);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("CreateLobby failed: " + ex);
        }
    }

    private async Task RefreshLobbyList()
    {
        // Clear list UI
        if (lobbyListParent != null)
        {
            foreach (Transform child in lobbyListParent)
                Destroy(child.gameObject);
        }

        try
        {
            // Simple query for public lobbies
            var queryOptions = new QueryLobbiesOptions();
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);

            foreach (var lobby in queryResponse.Results)
            {
                var go = Instantiate(lobbyButtonPrefab, lobbyListParent);
                var txt = go.GetComponentInChildren<Text>();
                if (txt != null) txt.text = lobby.Name + " (" + lobby.Players.Count + ")";

                // Get stored join code
                string joinCode = "";
                if (lobby.Data != null && lobby.Data.ContainsKey("joinCode"))
                    joinCode = lobby.Data["joinCode"].Value;

                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(async () => await JoinLobby(joinCode));
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("RefreshLobbyList failed: " + ex);
        }
    }

    private async Task JoinLobby(string joinCode)
    {
        if (string.IsNullOrEmpty(joinCode))
        {
            Debug.LogError("Join code is empty.");
            return;
        }

        try
        {
            // 1) Join the relay allocation via join code
            JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // 2) Configure transport for client using joinAlloc fields
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("UnityTransport not found on NetworkManager.");
                return;
            }

            transport.SetRelayServerData(
                joinAlloc.RelayServer.IpV4,
                (ushort)joinAlloc.RelayServer.Port,
                joinAlloc.AllocationIdBytes,
                joinAlloc.Key,
                joinAlloc.ConnectionData,
                joinAlloc.HostConnectionData
            );

            // 3) Start client
            NetworkManager.Singleton.StartClient();

            Debug.Log("Attempted to join lobby with code: " + joinCode);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JoinLobby failed: " + ex);
        }
    }
}
