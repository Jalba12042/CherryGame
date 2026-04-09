using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerEscapeUI : MonoBehaviour
{
    /*[Header("Player Panels")]
    public GameObject P1;
    public GameObject P2;
    public GameObject P3;
    public GameObject P4;*/

    [Header("Escape Settings")]
    public float mashFillSpeed = 0.2f;   // How much bar fills per press
    public float escapeThreshold = 1f;   // Fill required to escape

    private Gamepad assignedGamepad;
    private float fillAmount = 0f;
    private bool isBeingGrabbed = false;

    [Header("Player Info")]
    public int playerIndex = 0;

    // UI references for the current active panel
    private Image fillBar;
    private TextMeshProUGUI mashText;
    private Image yButtonIcon;
    //private GameObject currentPanel;
    public GameObject panelRoot;  

    private PlayerGrab grabbedBy; // the grabber

    void Start()
    {
        // Ensure the panel starts hidden
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }


    /*void Start()
    {
        EscapeUIInfo escapeUI = GameObject.FindWithTag("EscapeUI").GetComponent<EscapeUIInfo>();
        P1 = escapeUI.P1;
        P2 = escapeUI.P2;
        P3 = escapeUI.P3;
        P4 = escapeUI.P4;

        // Ensure all panels start disabled
        if (P1 != null) P1.SetActive(false);
        if (P2 != null) P2.SetActive(false);
        if (P3 != null) P3.SetActive(false);
        if (P4 != null) P4.SetActive(false);
    }*/

    // Called by grabber when player is grabbed
    // Called by grabber when player is grabbed
    public void StartBeingGrabbed(PlayerGrab grabber)
    {
        isBeingGrabbed = true;
        fillAmount = 0f;
        grabbedBy = grabber;

        if (assignedGamepad == null && GameManager.Instance != null)
        {
            int controllerIndex = GameManager.Instance.controllerAssignments[playerIndex];

            if (controllerIndex >= 0 && controllerIndex < Gamepad.all.Count)
            {
                assignedGamepad = Gamepad.all[controllerIndex];
            }
        }

        // Enable this player's world-space UI
        panelRoot.SetActive(true);

        // Cache UI
        fillBar = panelRoot.transform.Find("FillBar")?.GetComponent<Image>();
        mashText = panelRoot.transform.Find("MashText")?.GetComponent<TextMeshProUGUI>();
        yButtonIcon = panelRoot.transform.Find("YButton Image")?.GetComponent<Image>();

        if (fillBar != null)
            fillBar.fillAmount = 0f;
    }

    /*public void StartBeingGrabbed(PlayerGrab grabber)
    {
        isBeingGrabbed = true;
        fillAmount = 0f;
        grabbedBy = grabber;

        // ✅ Use the grabbed player's own index (not the grabber's)
        playerIndex = GetComponent<Playermovement>().playerIndex;

        // ✅ Assign the grabbed player's own gamepad
        if (Gamepad.all.Count > playerIndex)
            assignedGamepad = Gamepad.all[playerIndex];

        // ✅ Enable the correct panel for the grabbed player
        currentPanel = GetPanelForIndex(playerIndex);
        if (currentPanel != null)
        {
            currentPanel.SetActive(true);

            fillBar = currentPanel.transform.Find("FillBar")?.GetComponent<Image>();
            mashText = currentPanel.transform.Find("MashText")?.GetComponent<TextMeshProUGUI>();
            yButtonIcon = currentPanel.transform.Find("YButton Image")?.GetComponent<Image>();

            if (fillBar != null)
                fillBar.fillAmount = 0f;
        }

        Debug.Log($"[EscapeUI] StartBeingGrabbed: Player {playerIndex} (grabbed by Player {grabber.GetComponent<Playermovement>().playerIndex})");
    }*/


    // Called by grabber when player is released or escapes
    public void StopBeingGrabbed()
    {
        isBeingGrabbed = false;
        fillAmount = 0f;

        if (fillBar != null)
            fillBar.fillAmount = 0f;

        panelRoot.SetActive(false);
    }

    /*public void StopBeingGrabbed()
    {
        isBeingGrabbed = false;
        fillAmount = 0f;

        // Reset fill bar
        if (fillBar != null)
            fillBar.fillAmount = 0f;

        // Disable the active panel
        if (currentPanel != null)
            currentPanel.SetActive(false);

        Debug.Log($"[EscapeUI] StopBeingGrabbed: Player {playerIndex}");
    }*/

    void Update()
    {
        if (!isBeingGrabbed)
            return;

        /*if (assignedGamepad == null && Gamepad.all.Count > playerIndex)
            assignedGamepad = Gamepad.all[playerIndex];*/

        if (assignedGamepad == null)
            return;

        // Check Y button presses
        if (assignedGamepad.buttonNorth.wasPressedThisFrame)
        {
            fillAmount += mashFillSpeed;
            fillAmount = Mathf.Clamp(fillAmount, 0f, escapeThreshold);

            if (fillBar != null)
                fillBar.fillAmount = fillAmount / escapeThreshold;

            if (fillAmount >= escapeThreshold)
                Escape();
        }
    }

    private void Escape()
    {
        Debug.Log($"[EscapeUI] Escape triggered for grabbed player {playerIndex}, grabbed by player {grabbedBy.GetComponent<Playermovement>().playerIndex}");
        StopBeingGrabbed();

        // Tell grabber to release this player and start cooldown
        if (grabbedBy != null)
        {
            grabbedBy.StartCoroutine(grabbedBy.GrabCooldown());
            grabbedBy.ForceRelease();
        }
    }


    // Returns the corresponding panel for a given player index
    /*private GameObject GetPanelForIndex(int index)
    {
        return index switch
        {
            0 => P1,
            1 => P2,
            2 => P3,
            3 => P4,
            _ => null
        };
    }*/
}
