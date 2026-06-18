/*using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    public string JoinCode { get; private set; }
    public bool IsHost { get; private set; }

    public event Action<string> OnGameCreated;
    public event Action OnGameJoined;
    public event Action<string> OnConnectionFailed;

    private const int MaxConnections = 3;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async void EnsureServicesInitialized(Action onComplete = null)
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log("UGS ready. Player ID: " + AuthenticationService.Instance.PlayerId);
            onComplete?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError("UGS init failed: " + e.Message);
            OnConnectionFailed?.Invoke("Could not connect to Unity Services");
        }
    }

    public async void CreateGame()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections);

            JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));

            NetworkManager.Singleton.StartHost();
            IsHost = true;

            Debug.Log("Game created. Join code: " + JoinCode);
            OnGameCreated?.Invoke(JoinCode);
        }
        catch (Exception e)
        {
            Debug.LogError("Create game failed: " + e.Message);
            OnConnectionFailed?.Invoke("Failed to create game");
        }
    }

    public async void JoinGame(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            NetworkManager.Singleton.StartClient();
            JoinCode = joinCode;
            IsHost = false;

            Debug.Log("Joined game with code: " + joinCode);
            OnGameJoined?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError("Join failed: " + e.Message);
            OnConnectionFailed?.Invoke("Invalid join code or game not found");
        }
    }

    public void Disconnect()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();
        JoinCode = null;
        IsHost = false;
    }
}
*/