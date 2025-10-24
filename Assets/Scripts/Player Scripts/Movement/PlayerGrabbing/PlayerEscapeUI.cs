using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerEscapeUI : MonoBehaviour
{
    [Header("Player Panels")]
    public GameObject P1;
    public GameObject P2;
    public GameObject P3;
    public GameObject P4;

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
    private GameObject currentPanel;

    void Start()
    {
        // Ensure all panels start disabled
        if (P1 != null) P1.SetActive(false);
        if (P2 != null) P2.SetActive(false);
        if (P3 != null) P3.SetActive(false);
        if (P4 != null) P4.SetActive(false);
    }

    // Called by grabber when player is grabbed
    public void StartBeingGrabbed(int grabbedPlayerIndex)
    {
        isBeingGrabbed = true;
        fillAmount = 0f;
        playerIndex = grabbedPlayerIndex;

        // Assign gamepad
        if (Gamepad.all.Count > grabbedPlayerIndex)
            assignedGamepad = Gamepad.all[grabbedPlayerIndex];

        // Enable the correct panel
        currentPanel = GetPanelForIndex(grabbedPlayerIndex);
        if (currentPanel != null)
        {
            currentPanel.SetActive(true);

            // Cache panel UI references
            fillBar = currentPanel.transform.Find("FillBar")?.GetComponent<Image>();
            mashText = currentPanel.transform.Find("MashText")?.GetComponent<TextMeshProUGUI>();
            yButtonIcon = currentPanel.transform.Find("YButton Image")?.GetComponent<Image>();

            // Reset fill bar
            if (fillBar != null) fillBar.fillAmount = 0f;
        }

        Debug.Log($"[EscapeUI] StartBeingGrabbed: Player {grabbedPlayerIndex}");
    }

    // Called by grabber when player is released or escapes
    public void StopBeingGrabbed()
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
    }

    void Update()
    {
        if (!isBeingGrabbed)
            return;

        if (assignedGamepad == null && Gamepad.all.Count > playerIndex)
            assignedGamepad = Gamepad.all[playerIndex];

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
        Debug.Log($"[EscapeUI] Escape triggered for Player {playerIndex}");
        StopBeingGrabbed();

        // Tell grabber to release this player
        var grabbedBy = GetComponent<PlayerGrabbed>();
        if (grabbedBy != null && grabbedBy.grabber != null)
        {
            grabbedBy.grabber.StartCoroutine(grabbedBy.grabber.GrabCooldown());
            grabbedBy.grabber.HandlePlayerRelease();

            // Re-enable PlayerPickup if needed
            PlayerPickup pickup = GetComponent<PlayerPickup>() ?? GetComponentInChildren<PlayerPickup>();
            if (pickup != null)
                pickup.enabled = true;
        }
    }

    // Returns the corresponding panel for a given player index
    private GameObject GetPanelForIndex(int index)
    {
        return index switch
        {
            0 => P1,
            1 => P2,
            2 => P3,
            3 => P4,
            _ => null
        };
    }
}
