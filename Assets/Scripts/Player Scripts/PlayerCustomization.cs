using Unity.Netcode;
using UnityEngine;

public class PlayerCustomization : NetworkBehaviour
{
    // Server-authoritative copy of this player's cosmetics, so every peer (not just the
    // owner) renders the correct look. Local/offline play never spawns this as a
    // NetworkObject, so it's simply unused there and ApplyFromData keeps working directly.
    public NetworkVariable<PlayerCustomizationData> NetworkCustomization = new NetworkVariable<PlayerCustomizationData>(
        PlayerCustomizationData.Unset, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        NetworkCustomization.OnValueChanged += OnNetworkCustomizationChanged;
        if (!NetworkCustomization.Value.Equals(PlayerCustomizationData.Unset))
            ApplyFromData(NetworkCustomization.Value);
    }

    public override void OnNetworkDespawn()
    {
        NetworkCustomization.OnValueChanged -= OnNetworkCustomizationChanged;
    }

    private void OnNetworkCustomizationChanged(PlayerCustomizationData previous, PlayerCustomizationData current)
    {
        ApplyFromData(current);
    }

    // Called server-side (e.g. by the spawner) once this player's chosen cosmetics are known.
    public void SetNetworkCustomization(PlayerCustomizationData data)
    {
        if (IsServer) NetworkCustomization.Value = data;
    }

    public Renderer bodyRenderer;

    public GameObject[] headOptions;
    public GameObject[] faceOptions;
    public GameObject[] torsoOptions;
    public GameObject[] bottomOptions;
    public int playerIndex; 


    public Material[] colorMaterials;

    // Used by RoundManager to auto-assign a player-identity color on spawn
    public Material[] playerMaterials;

    private int currentHeadIndex = -1;
    public int currentFaceIndex = -1;
    private int currentTorsoIndex = -1;
    private int currentBottomIndex = -1;

    public int GetHeadIndex() => currentHeadIndex;
    public int GetFaceIndex() => currentFaceIndex;
    public int GetTorsoIndex() => currentTorsoIndex;
    public int GetBottomIndex() => currentBottomIndex;

    public int CurrentColorIndex { get; private set; }

    public void Initialize()
    {
        // Disable everything at start
        DisableAll(headOptions);
        DisableAll(faceOptions);
        DisableAll(torsoOptions);
        DisableAll(bottomOptions);
    }

    void DisableAll(GameObject[] options)
    {
        if (options == null) return;
        foreach (var go in options)
        {
            if (go != null)
                go.SetActive(false);
        }
    }

    public void EnableOne(GameObject[] options, int index)
    {
        if (options == null) return;

        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != null)
                options[i].SetActive(i == index);
        }
    }

    public void ApplyBodyMaterial(Material mat, int colorIndex)
    {
        if (bodyRenderer != null && mat != null)
        {
            bodyRenderer.material = mat;
            CurrentColorIndex = colorIndex;
        }
    }

    public void ChangeHead(int dir)
    {
        if (headOptions == null || headOptions.Length == 0) return;

        int max = headOptions.Length - 1;

        currentHeadIndex += dir;

        if (currentHeadIndex > max)
            currentHeadIndex = -1; // wrap to NONE
        else if (currentHeadIndex < -1)
            currentHeadIndex = max;

        EnableOne(headOptions, currentHeadIndex);
    }

    public void ChangeFace(int dir)
    {
        if (faceOptions == null || faceOptions.Length == 0) return;

        int max = faceOptions.Length - 1;

        currentFaceIndex += dir;

        if (currentFaceIndex > max)
            currentFaceIndex = -1; // wrap to NONE
        else if (currentFaceIndex < -1)
            currentFaceIndex = max;

        EnableOne(faceOptions, currentFaceIndex);
    }

    public void ChangeTorso(int dir)
    {
        if (torsoOptions == null || torsoOptions.Length == 0) return;

        int max = torsoOptions.Length - 1;

        currentTorsoIndex += dir;

        if (currentTorsoIndex > max)
            currentTorsoIndex = -1;
        else if (currentTorsoIndex < -1)
            currentTorsoIndex = max;

        EnableOne(torsoOptions, currentTorsoIndex);
    }

    public void ChangeBottom(int dir)
    {
        if (bottomOptions == null || bottomOptions.Length == 0) return;

        int max = bottomOptions.Length - 1;

        currentBottomIndex += dir;

        if (currentBottomIndex > max)
            currentBottomIndex = -1;
        else if (currentBottomIndex < -1)
            currentBottomIndex = max;

        EnableOne(bottomOptions, currentBottomIndex);
    }

    // Replaces PlayerColorAssigner.AssignColor()
    public void AssignColor(int playerIndex)
    {
        if (playerMaterials == null || playerIndex >= playerMaterials.Length) return;
        ApplyBodyMaterial(playerMaterials[playerIndex], playerIndex);
    }

    public void ApplyFromData(PlayerCustomizationData data)
    {
        Initialize();

        // HEAD
        if (data.headIndex >= 0)
            EnableOne(headOptions, data.headIndex);

        // FACE
        if (data.faceIndex >= 0)
            EnableOne(faceOptions, data.faceIndex);

        // TORSO
        if (data.torsoIndex >= 0)
            EnableOne(torsoOptions, data.torsoIndex);

        // BOTTOM
        if (data.bottomIndex >= 0)
            EnableOne(bottomOptions, data.bottomIndex);

        // COLOR
        if (colorMaterials != null && data.colorIndex >= 0 && data.colorIndex < colorMaterials.Length)
        {
            ApplyBodyMaterial(colorMaterials[data.colorIndex], data.colorIndex);
        }
    }
}