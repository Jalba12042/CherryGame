using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class AnyInputToAnimatorTrigger : MonoBehaviour
{
    [Header("Animators to Trigger")]
    public Animator titleAnimator;   // FEUD title animator
    public Animator promptAnimator;  // "PRESS ANY BUTTON" animator
    public string triggerName = "Fade";

    [Header("Menu Objects")]
    public GameObject menuManager;   // Clouds, hill, and signs parent
    public MainMenuController menuController; // Script that handles A-button menu navigation

    private bool fired = false;

    void Start()
    {
        // Hide main menu visuals and disable input at the start
        if (menuManager != null)
            menuManager.SetActive(false);

        if (menuController != null)
            menuController.enabled = false;
    }

    void Update()
    {
        if (fired) return;

        if (AnyInput())
        {
            fired = true;
            TriggerFadeAnimations();
            StartCoroutine(WaitThenUnlock());
        }
    }

    void TriggerFadeAnimations()
    {
        if (titleAnimator != null)
            titleAnimator.SetTrigger(triggerName);

        if (promptAnimator != null)
            promptAnimator.SetTrigger(triggerName);
    }

    IEnumerator WaitThenUnlock()
    {
        // Wait a little longer than your fade duration (1 second = adjust if needed)
        yield return new WaitForSeconds(1.2f);

        // Fade finished — show menu visuals and unlock controller input
        if (menuManager != null)
            menuManager.SetActive(true);

        if (menuController != null)
            menuController.enabled = true;
    }

    bool AnyInput()
    {
        // Keyboard
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        // Mouse
        if (Mouse.current != null &&
            (Mouse.current.leftButton.wasPressedThisFrame ||
             Mouse.current.rightButton.wasPressedThisFrame ||
             Mouse.current.middleButton.wasPressedThisFrame))
            return true;

        // Gamepad (Xbox, PS, etc.)
        var pad = Gamepad.current;
        if (pad != null)
        {
            if (pad.buttonSouth.wasPressedThisFrame || pad.buttonEast.wasPressedThisFrame ||
                pad.buttonWest.wasPressedThisFrame || pad.buttonNorth.wasPressedThisFrame ||
                pad.startButton.wasPressedThisFrame || pad.selectButton.wasPressedThisFrame ||
                pad.leftShoulder.wasPressedThisFrame || pad.rightShoulder.wasPressedThisFrame ||
                pad.leftTrigger.wasPressedThisFrame || pad.rightTrigger.wasPressedThisFrame)
                return true;

            if (pad.dpad.ReadValue() != Vector2.zero)
                return true;

            if (pad.leftStick.ReadValue().sqrMagnitude > 0.25f)
                return true;
        }

        return false;
    }
}
