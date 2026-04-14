using UnityEngine;

public class PlayerCustomization : MonoBehaviour
{
    public Renderer bodyRenderer;

    public GameObject[] headOptions;
    public GameObject[] torsoOptions;
    public GameObject[] bottomOptions;
    public int playerIndex; 


    public Material[] colorMaterials;

    private int currentHeadIndex = -1;
    private int currentTorsoIndex = -1;
    private int currentBottomIndex = -1;

    public int GetHeadIndex() => currentHeadIndex;
    public int GetTorsoIndex() => currentTorsoIndex;
    public int GetBottomIndex() => currentBottomIndex;

    public void Initialize()
    {
        // Disable everything at start
        DisableAll(headOptions);
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

    public void ApplyBodyMaterial(Material mat)
    {
        if (bodyRenderer != null && mat != null)
            bodyRenderer.material = mat;
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

    public void ApplyFromData(PlayerCustomizationData data)
    {
        Initialize();

        // HEAD
        if (data.headIndex >= 0)
            EnableOne(headOptions, data.headIndex);

        // TORSO
        if (data.torsoIndex >= 0)
            EnableOne(torsoOptions, data.torsoIndex);

        // BOTTOM
        if (data.bottomIndex >= 0)
            EnableOne(bottomOptions, data.bottomIndex);

        // COLOR
        if (colorMaterials != null && data.colorIndex >= 0 && data.colorIndex < colorMaterials.Length)
        {
            ApplyBodyMaterial(colorMaterials[data.colorIndex]);
        }
    }
}