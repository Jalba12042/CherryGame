using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerEscapeUI : MonoBehaviour
{
    [Header("UI References")]
    public Canvas escapeCanvas;          // World-space canvas (can be disabled in inspector)
    public Image fillBar;                // The fill image (Image.type = Filled)
    public TextMeshProUGUI mashText;     // Optional: "MASH" text
    public Image aButtonIcon;            // Image of A button

    [Header("Escape Settings")]
    public float mashFillSpeed = 0.2f;   // How much bar fills per press
    public float escapeThreshold = 1f;   // Fill required to escape

    private Gamepad assignedGamepad;
    private float fillAmount = 0f;
    private bool isBeingGrabbed = false;

    [Header("Player Info")]
    public int playerIndex = 0;

    // Called by grabber to start the escape UI. This must work even if this
    // component's GameObject (or canvas) was disabled at scene start.
    public void StartBeingGrabbed(int grabbedPlayerIndex)
    {
        isBeingGrabbed = true;
        fillAmount = 0f;
        if (fillBar != null) fillBar.fillAmount = 0f;

        // Set our playerIndex so logging and fallback re-enables are accurate
        playerIndex = grabbedPlayerIndex;

        // Assign the gamepad of the player being grabbed
        if (Gamepad.all.Count > grabbedPlayerIndex)
            assignedGamepad = Gamepad.all[grabbedPlayerIndex];

        if (escapeCanvas != null)
            escapeCanvas.gameObject.SetActive(true);

        Debug.Log($"[EscapeUI] StartBeingGrabbed called — grabbed player index: {grabbedPlayerIndex}");
    }

    // Called by grabber to hide/reset the UI
    public void StopBeingGrabbed()
    {
        isBeingGrabbed = false;
        fillAmount = 0f;

        if (fillBar != null)
            fillBar.fillAmount = 0f;

        if (escapeCanvas != null)
            Debug.Log($"[EscapeUI] HideUI check — player index {playerIndex} has escaped (UI will now hide).");
        escapeCanvas.gameObject.SetActive(false);

        Debug.Log($"[EscapeUI] StopBeingGrabbed called — player index {playerIndex} has escaped / UI hidden");
    }

    void Update()
    {
        if (!isBeingGrabbed)
            return;

        // Always use this player's own gamepad
        if (assignedGamepad == null && Gamepad.all.Count > playerIndex)
            assignedGamepad = Gamepad.all[playerIndex];

        if (assignedGamepad == null)
            return;

        // Check for North button presses (Y button)
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
        Debug.Log($"[EscapeUI] Escape() triggered for player index {playerIndex}");

        // Reset/hide UI
        StopBeingGrabbed();

        // Tell the grabbing player to release this player
        var grabbedBy = GetComponent<PlayerGrabbed>();
        if (grabbedBy != null && grabbedBy.grabber != null)
        {
            // Disable the grabber temporarily so they can’t immediately grab again
            grabbedBy.grabber.StartCoroutine(grabbedBy.grabber.GrabCooldown());

            // Release this player
            grabbedBy.grabber.HandlePlayerRelease();

            // Try to re-enable our own PlayerPickup component (safeguard)
            PlayerPickup myPickup = GetComponent<PlayerPickup>();
            if (myPickup != null)
            {
                myPickup.enabled = true;
                Debug.Log($"[EscapeUI] Re-enabled PlayerPickup on player index {playerIndex} (component on same GameObject).");
            }
            else
            {
                // If PlayerPickup is on a child, try to find it
                PlayerPickup childPickup = GetComponentInChildren<PlayerPickup>();
                if (childPickup != null)
                {
                    childPickup.enabled = true;
                    Debug.Log($"[EscapeUI] Re-enabled PlayerPickup on player index {playerIndex} (found in children).");
                }
                else
                {
                    Debug.LogWarning($"[EscapeUI] Could NOT find PlayerPickup to re-enable for player index {playerIndex}.");
                }
            }

            Debug.Log($"Player {grabbedBy.grabber.playerIndex} released their grabbed player!");
        }
        else
        {
            Debug.LogWarning($"[EscapeUI] Escape(): no PlayerGrabbed or no grabber found on player index {playerIndex}.");
        }
    }




    void Start()
    {
        // If escapeCanvas is assigned but active, keep it inactive by default
        if (escapeCanvas != null)
            escapeCanvas.gameObject.SetActive(false);

        if (fillBar != null)
            fillBar.fillAmount = 0f;
    }
}
