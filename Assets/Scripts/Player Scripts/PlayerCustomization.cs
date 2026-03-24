using UnityEngine;

public class PlayerCustomization : MonoBehaviour
{
    public GameObject[] headOptions;
    public GameObject[] torsoOptions;
    public GameObject[] bottomOptions;

    private int currentHeadIndex;
    private int currentTorsoIndex;
    private int currentBottomIndex;

    public void Initialize()
    {
        EnableOne(headOptions, currentHeadIndex);
        EnableOne(torsoOptions, currentTorsoIndex);
        EnableOne(bottomOptions, currentBottomIndex);
    }

    void EnableOne(GameObject[] options, int index)
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != null)
                options[i].SetActive(i == index);
        }
    }

    public void ChangeHead(int dir)
    {
        if (headOptions.Length == 0) return;

        currentHeadIndex = (currentHeadIndex + dir + headOptions.Length) % headOptions.Length;
        EnableOne(headOptions, currentHeadIndex);
    }

    public void ChangeTorso(int dir)
    {
        currentTorsoIndex = (currentTorsoIndex + dir + torsoOptions.Length) % torsoOptions.Length;
        EnableOne(torsoOptions, currentTorsoIndex);
    }

    public void ChangeBottom(int dir)
    {
        currentBottomIndex = (currentBottomIndex + dir + bottomOptions.Length) % bottomOptions.Length;
        EnableOne(bottomOptions, currentBottomIndex);
    }
}