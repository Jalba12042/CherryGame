using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class ControllerLayoutBack : MonoBehaviour
{
    [Header("Play exit via this orchestrator")]
    public MenuExitOrchestrator exitOrchestrator;   // point to your existing one
    public string sceneToLoad = "Main Menu";        // Title scene name

    [Header("When can player press Back?")]
    public bool armOnStart = false;                 // set true to use a simple delay
    public float delayBeforeInput = 0.8f;           // used only if armOnStart = true

    private bool armed = false;
    private bool fired = false;

    void Start()
    {
        if (armOnStart)
            StartCoroutine(ArmAfterDelay(delayBeforeInput));
        // Otherwise: call ArmInput() from an Animation Event at the end of your layout intro
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

            if (exitOrchestrator != null)
                exitOrchestrator.ExitThenLoad(sceneToLoad);
            else
                SceneManager.LoadScene(sceneToLoad); // fallback if you forgot to wire it
        }
    }

    bool BackPressed()
    {
        // Keyboard “back” keys
        if (Keyboard.current != null &&
           (Keyboard.current.escapeKey.wasPressedThisFrame ||
            Keyboard.current.backspaceKey.wasPressedThisFrame))
            return true;

        // Gamepad “B / Circle” is typical back; allow Start/Select too if you want
        var pad = Gamepad.current;
        if (pad != null)
            return pad.buttonEast.wasPressedThisFrame ||
                   pad.startButton.wasPressedThisFrame ||
                   pad.selectButton.wasPressedThisFrame;

        return false;
    }
}
