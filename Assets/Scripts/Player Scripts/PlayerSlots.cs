using UnityEngine;
using UnityEngine.UI;

public class PlayerSlots : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject joinPanel;   // P1Join
    public GameObject menuPanel;   // MenuArea
    public GameObject readyPanel;  // P1isReady

    [Header("Preview")]
    public RawImage previewImage;
    public Camera previewCamera;
    public Transform modelSpawnPoint;

    [Header("Customization UI")]
    public PlayerCustomizationUI customizationUI;

    [HideInInspector]
    public GameObject spawnedModel;
    [HideInInspector] public bool stickInUse = false;
}
