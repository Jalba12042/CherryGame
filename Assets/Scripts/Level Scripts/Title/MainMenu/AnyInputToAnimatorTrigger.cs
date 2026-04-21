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
    public GameObject[] environmentObjects;

    [Header("Timing")]
    public float fadeOutWaitSeconds = 1.0f;
    // How many seconds to wait before the player is allowed to press a button
    public float inputDelaySeconds = 2.0f;

    [Header("Arming input")]
    public bool armInputOnStart = true; // Kept this true so it starts automatically

    [Header("Audio")]
    public AudioSource audioSource; // Drag an AudioSource here
    public AudioClip startSound;    // Drag your "Press Start" sound effect here

    private bool inputArmed = false;
    private bool fired = false;

    void Start()
    {
        if (menuManager) menuManager.SetActive(false);
        if (menuController) menuController.enabled = false;

        if (environmentObjects != null)
        {
            foreach (GameObject obj in environmentObjects)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        if (armInputOnStart)
        {
            // Instead of arming instantly, start the delay timer
            StartCoroutine(ArmInputAfterDelay());
        }
    }

    // The timer that waits before letting the player press a button
    IEnumerator ArmInputAfterDelay()
    {
        yield return new WaitForSeconds(inputDelaySeconds);
        inputArmed = true;
    }

    public void ArmInput()
    {
        inputArmed = true;
    }

    void Update()
    {
        if (!inputArmed || fired) return;

        if (AnyInput())
        {
            fired = true;

            // --- NEW: Play the sound effect! ---
            if (audioSource != null && startSound != null)
            {
                audioSource.PlayOneShot(startSound);
            }

            if (titleAnimator) titleAnimator.SetTrigger(triggerName);
            if (promptAnimator) promptAnimator.SetTrigger(triggerName);

            StartCoroutine(RevealAfterDelay());
        }
    }

    IEnumerator RevealAfterDelay()
    {
        yield return new WaitForSecondsRealtime(fadeOutWaitSeconds);
        RevealMenu();
    }

    public void RevealMenu()
    {
        if (menuManager) menuManager.SetActive(true);
        if (menuController) menuController.enabled = true;

        if (environmentObjects != null)
        {
            foreach (GameObject obj in environmentObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
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