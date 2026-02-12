using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;

public class LobbyManager : MonoBehaviour
{
    public ISession CurrentSession { get; private set; }

    async void Awake()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"UGS initialized (PlayerID={AuthenticationService.Instance.PlayerId})");
        }
        catch (Exception e)
        {
            Debug.LogError($"UGS init failed: {e}");
        }
    }

    public async Task CreateLobby(bool isPublic)
    {
        try
        {
            var options = new SessionOptions
            {
                MaxPlayers = 10,
                IsPrivate = !isPublic,
                Name = "MyLobby"
            }.WithRelayNetwork(); // adjust to your network type

            CurrentSession = await MultiplayerService.Instance.CreateSessionAsync(options);

            string joinCode = TryGetJoinCode(CurrentSession);
            Debug.Log($"Created session {CurrentSession.Id} | join code: {(string.IsNullOrEmpty(joinCode) ? "N/A" : joinCode)}");
        }
        catch (Exception e)
        {
            Debug.LogError($"CreateLobby failed: {e}");
        }
    }

    public async Task JoinLobbyByCode(string code)
    {
        try
        {
            CurrentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);
            Debug.Log($"Joined session {CurrentSession.Id} via code");
        }
        catch (Exception e)
        {
            Debug.LogError($"JoinLobbyByCode failed: {e}");
        }
    }

    public async Task JoinLobbyById(string sessionId)
    {
        try
        {
            CurrentSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId);
            Debug.Log($"Joined session {CurrentSession.Id} via id");
        }
        catch (Exception e)
        {
            Debug.LogError($"JoinLobbyById failed: {e}");
        }
    }

    public async Task QueryPublicLobbies(int count = 20)
    {
        try
        {
            var query = new QuerySessionsOptions
            {
                Count = count
                // Removed MinAvailableSlots for compatibility
            };

            var result = await MultiplayerService.Instance.QuerySessionsAsync(query);

            if (result?.Sessions == null || result.Sessions.Count == 0)
            {
                Debug.Log("No public lobbies found.");
                return;
            }

            foreach (ISessionInfo info in result.Sessions)
            {
                // Use safe field access only
                int currentPlayers = EstimatePlayerCount(info);
                Debug.Log($"Lobby: Id={info.Id}, Name={info.Name}, Players={currentPlayers}/{info.MaxPlayers}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"QueryPublicLobbies failed: {e}");
        }
    }

    // --- helpers ---
    private static string TryGetJoinCode(ISession session)
    {
        if (session == null) return null;
        var prop = session.GetType().GetProperty("Code", BindingFlags.Instance | BindingFlags.Public);
        return prop?.GetValue(session) as string;
    }

    private static int EstimatePlayerCount(ISessionInfo info)
    {
        // Some builds have AvailableSlots, others don't.
        var prop = info.GetType().GetProperty("AvailableSlots", BindingFlags.Instance | BindingFlags.Public);
        if (prop != null && prop.PropertyType == typeof(int))
        {
            int available = (int)prop.GetValue(info);
            return info.MaxPlayers - available;
        }
        return 0; // fallback if field not exposed
    }
}
