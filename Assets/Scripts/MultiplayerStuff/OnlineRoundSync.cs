using Unity.Netcode;
using UnityEngine;

// Replicates the state RoundManager's timer coroutine already tracks locally, so online
// clients (who don't run that coroutine themselves - only the server does) can drive their
// own UI/gameplay gating from the same authoritative numbers.
public class OnlineRoundSync : NetworkBehaviour
{
    public static OnlineRoundSync Instance;

    public readonly NetworkVariable<float> RoundProgress = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public readonly NetworkVariable<float> RoundDuration = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public readonly NetworkVariable<bool> RoundActive = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public readonly NetworkVariable<bool> PlayersCanMove = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public readonly NetworkList<int> Scores = new NetworkList<int>();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this) Instance = null;
    }

    public void SetScores(int[] scores)
    {
        if (!IsServer) return;
        Scores.Clear();
        for (int i = 0; i < scores.Length; i++) Scores.Add(scores[i]);
    }
}
