using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerEscapeUI : MonoBehaviour
{
    [Header("UI References")]
    public Canvas escapeCanvas;          // World-space canvas
    public Image fillBar;                // The fill image
    public TextMeshProUGUI mashText;    // Optional: "MASH" text
    public Image aButtonIcon;            // Image of A button

    [Header("Escape Settings")]
    public float mashFillSpeed = 0.2f;   // How much bar fills per press
    public float escapeThreshold = 1f;   // Fill required to escape

    private Gamepad assignedGamepad;
    private float fillAmount = 0f;
    private bool isBeingGrabbed = false;

    [Header("Player Info")]
    public int playerIndex = 0;

    void Start()
    {
        if (Gamepad.all.Count > playerIndex)
            assignedGamepad = Gamepad.all[playerIndex];

        // Hide canvas initially
        if (escapeCanvas != null)
            escapeCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isBeingGrabbed || assignedGamepad == null) return;

        // Show UI
        if (!escapeCanvas.gameObject.activeSelf)
            escapeCanvas.gameObject.SetActive(true);

        // Check for A button press (South button)
        if (assignedGamepad.buttonSouth.wasPressedThisFrame)
        {
            fillAmount += mashFillSpeed;
            fillAmount = Mathf.Clamp(fillAmount, 0f, escapeThreshold);

            if (fillBar != null)
                fillBar.fillAmount = fillAmount / escapeThreshold;

            // Escape!
            if (fillAmount >= escapeThreshold)
            {
                Escape();
            }
        }
    }

    public void StartBeingGrabbed()
    {
        isBeingGrabbed = true;
        fillAmount = 0f;
        if (fillBar != null) fillBar.fillAmount = 0f;
        if (escapeCanvas != null) escapeCanvas.gameObject.SetActive(true);
    }

    public void StopBeingGrabbed()
    {
        isBeingGrabbed = false;
        fillAmount = 0f;
        if (fillBar != null) fillBar.fillAmount = 0f;
        if (escapeCanvas != null) escapeCanvas.gameObject.SetActive(false);
    }

    private void Escape()
    {
        StopBeingGrabbed();
        // Tell the grabbing player to release this player
        var grabber = GetComponent<PlayerGrabbed>();
        if (grabber != null)
            grabber.ReleaseGrabbedPlayer();
    }
}
