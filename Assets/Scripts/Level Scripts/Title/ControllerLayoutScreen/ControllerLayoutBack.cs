using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class ControllerLayoutBack : MonoBehaviour
{
    [Header("1. B Button Animation (Plays FIRST)")]
    public Animator bButtonAnimator;          // Drag the B Button object here!
    public string bButtonTrigger = "Exit";    // Ensure your animator uses this trigger name
    public float delayAfterBPressed = 0.3f;   // How long to wait before the rest of the screen fades

    [Header("2. Play exit via this orchestrator (Plays SECOND)")]
    public MenuExitOrchestrator exitOrchestrator;
    public string sceneToLoad = "Main Menu";

    [Header("When can player press Back?")]
    public bool armOnStart = false;
    public float delayBeforeInput = 0.8f;

    private bool armed = false;
    private bool fired = false;

    void Start()
    {
        if (armOnStart)
            StartCoroutine(ArmAfterDelay(delayBeforeInput));
    }

    IEnumerator ArmAfterDelay(float s)
    {
        yield return new WaitForSeconds(s);
        ArmInput();
    }

    // Call from Animation Event on the last frame of the ControllerLayout "intro" clip
    public void ArmInput()
    {
        armed = true;
    }

    void Update()
    {
        if (!armed || fired) return;

        if (BackPressed())
        {
            fired = true;

            // Start the custom 2-step exit sequence!
            StartCoroutine(ExitSequence());
        }
    }

    IEnumerator ExitSequence()
    {
        // STEP 1: Play the B Button fade out animation first!
        if (bButtonAnimator != null)
        {
            bButtonAnimator.SetTrigger(bButtonTrigger);
        }

        // STEP 2: Wait for a split second so the player sees the B button fade
        yield return new WaitForSeconds(delayAfterBPressed);

        // STEP 3: Trigger the rest of the screen to fade out and load the scene
        if (exitOrchestrator != null)
            exitOrchestrator.ExitThenLoad(sceneToLoad);
        else
            SceneManager.LoadScene(sceneToLoad); // fallback
    }

    bool BackPressed()
    {
        // Keyboard “back” keys
        if (Keyboard.current != null &&
           (Keyboard.current.escapeKey.wasPressedThisFrame ||
            Keyboard.current.backspaceKey.wasPressedThisFrame))
            return true;

        // Gamepad “B / Circle” is typical back
        var pad = Gamepad.current;
        if (pad != null)
            return pad.buttonEast.wasPressedThisFrame ||
                   pad.startButton.wasPressedThisFrame ||
                   pad.selectButton.wasPressedThisFrame;

        return false;
    }
}