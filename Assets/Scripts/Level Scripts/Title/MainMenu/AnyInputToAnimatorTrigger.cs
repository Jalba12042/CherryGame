using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class AnyInputToAnimatorTrigger : MonoBehaviour
{
    [Header("Animators to Trigger (fade OUT)")]
    public Animator titleAnimator;
    public Animator promptAnimator;
    public string triggerName = "Fade";

    [Header("Menu Objects (shown AFTER fades)")]
    public GameObject menuManager;
    public MainMenuController menuController;

    [Header("Timing")]
    public float fadeOutWaitSeconds = 1.0f;   // ≈ fade length

    [Header("Arming input")]
    public bool armInputOnStart = false;      // turn ON to skip the ArmInput() event requirement

    private bool inputArmed = false;
    private bool fired = false;

    void Start()
    {
        if (menuManager) menuManager.SetActive(false);
        if (menuController) menuController.enabled = false;

        if (armInputOnStart)
        {
            inputArmed = true;
            Debug.Log("[AnyInputToAnimatorTrigger] Input armed on Start.");
        }
        else
        {
            Debug.Log("[AnyInputToAnimatorTrigger] Waiting for ArmInput() Animation Event…");
        }
    }

    // Call this from your INTRO clip (last frame) when “PRESS ANY BUTTON” is visible
    public void ArmInput()
    {
        inputArmed = true;
        Debug.Log("[AnyInputToAnimatorTrigger] Input armed via Animation Event.");
    }

    void Update()
    {
        if (!inputArmed || fired) return;

        if (AnyInput())
        {
            fired = true;
            Debug.Log("[AnyInputToAnimatorTrigger] Any input detected → fading title/prompt.");
            if (titleAnimator) titleAnimator.SetTrigger(triggerName);
            if (promptAnimator) promptAnimator.SetTrigger(triggerName);

            StartCoroutine(RevealAfterDelay());
        }
    }

    IEnumerator RevealAfterDelay()
    {
        yield return new WaitForSeconds(fadeOutWaitSeconds);
        RevealMenu();
    }

    // (You can also call this from the end of your fade anim via Animation Event)
    public void RevealMenu()
    {
        if (menuManager) menuManager.SetActive(true);

        if (menuController)
        {
            menuController.enabled = true;
            Debug.Log("[AnyInputToAnimatorTrigger] MenuController enabled.");
        }

        enabled = false;
    }

    bool AnyInput()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) return true;

        if (Mouse.current != null &&
            (Mouse.current.leftButton.wasPressedThisFrame ||
             Mouse.current.rightButton.wasPressedThisFrame ||
             Mouse.current.middleButton.wasPressedThisFrame)) return true;

        var pad = Gamepad.current;
        if (pad != null)
        {
            if (pad.buttonSouth.wasPressedThisFrame || pad.buttonEast.wasPressedThisFrame ||
                pad.buttonWest.wasPressedThisFrame || pad.buttonNorth.wasPressedThisFrame ||
                pad.startButton.wasPressedThisFrame || pad.selectButton.wasPressedThisFrame ||
                pad.leftShoulder.wasPressedThisFrame || pad.rightShoulder.wasPressedThisFrame ||
                pad.leftTrigger.wasPressedThisFrame || pad.rightTrigger.wasPressedThisFrame)
                return true;

            if (pad.dpad.ReadValue() != Vector2.zero) return true;
            if (pad.leftStick.ReadValue().sqrMagnitude > 0.25f) return true;
        }
        return false;
    }
}
