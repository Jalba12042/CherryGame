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
    public Button createLobbyButton;
    public Button refreshLobbyButton;
    public Transform lobbyListParent;
    public GameObject lobbyButtonPrefab;

    private Lobby currentLobby;
    private bool isSignedIn = false;
    private float heartbeatTimer;

    private void Awake()
    {
        createLobbyButton?.onClick.AddListener(async () => await CreateLobby());
        refreshLobbyButton?.onClick.AddListener(async () => await RefreshLobbyList());
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        isSignedIn = true;
        Debug.Log("Signed in: " + AuthenticationService.Instance.PlayerId);
    }

    private void Update()
    {
        if (currentLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0f)
            {
                heartbeatTimer = 15f;
                _ = LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            }
        }
    }

    private async Task CreateLobby()
    {
        if (!isSignedIn) return;

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                }
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync("MyLobby", 4, options);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.ConnectionData
            );

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
        if (!isSignedIn) return;

        if (lobbyListParent != null)
        {
            foreach (Transform child in lobbyListParent)
                Destroy(child.gameObject);
        }

        try
        {
            var queryOptions = new QueryLobbiesOptions { Count = 10 };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);

            foreach (var lobby in queryResponse.Results)
            {
                var go = Instantiate(lobbyButtonPrefab, lobbyListParent);
                var txt = go.GetComponentInChildren<Text>();
                if (txt != null) txt.text = lobby.Name + " (" + lobby.Players.Count + "/" + lobby.MaxPlayers + ")";

                if (lobby.Data != null && lobby.Data.ContainsKey("joinCode"))
                {
                    string joinCode = lobby.Data["joinCode"].Value;
                    var btn = go.GetComponent<Button>();
                    if (btn != null)
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
        if (!isSignedIn || string.IsNullOrEmpty(joinCode)) return;

        try
        {
            JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                joinAlloc.RelayServer.IpV4,
                (ushort)joinAlloc.RelayServer.Port,
                joinAlloc.AllocationIdBytes,
                joinAlloc.Key,
                joinAlloc.ConnectionData,
                joinAlloc.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
            Debug.Log("Joined lobby with code: " + joinCode);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JoinLobby failed: " + ex);
        }
    }
}
