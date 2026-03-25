using UnityEngine;

public class PlayerCustomization : MonoBehaviour
{
    public GameObject[] headOptions;
    public GameObject[] torsoOptions;
    public GameObject[] bottomOptions;

    private int currentHeadIndex = -1;
    private int currentTorsoIndex = -1;
    private int currentBottomIndex = -1;



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

    void EnableOne(GameObject[] options, int index)
    {
        if (options == null || index < 0 || index >= options.Length) return;
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != null)
                options[i].SetActive(i == index);
        }
    }

    public void ChangeHead(int dir)
    {
        if (headOptions == null || headOptions.Length == 0) return;

        currentHeadIndex = (currentHeadIndex + dir + headOptions.Length) % headOptions.Length;
        EnableOne(headOptions, currentHeadIndex);
    }

    public void ChangeTorso(int dir)
    {
        if (torsoOptions == null || torsoOptions.Length == 0) return;

        currentTorsoIndex = (currentTorsoIndex + dir + torsoOptions.Length) % torsoOptions.Length;
        EnableOne(torsoOptions, currentTorsoIndex);
    }

    public void ChangeBottom(int dir)
    {
        if (bottomOptions == null || bottomOptions.Length == 0) return;

        currentBottomIndex = (currentBottomIndex + dir + bottomOptions.Length) % bottomOptions.Length;
        EnableOne(bottomOptions, currentBottomIndex);
    }
}