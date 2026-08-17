using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class ControllerLayoutBack : MonoBehaviour
{
    [Header("1. B Button Animation (Plays FIRST)")]
    public Animator bButtonAnimator;
    public string bButtonTrigger = "Exit";
    public float delayAfterBPressed = 0.3f;

    [Header("2. Paper Transition (Plays SECOND)")]
    public Animator paperTransitionAnimator;
    public string paperReverseTriggerName = "PaperReverseTrigger";
    public float paperAnimationDuration = 1.0f;
    public string sceneToLoad = "Main Menu";

    [Header("UI Objects to Hide")]
    [Tooltip("Drag the PaperOverlay, PaperBG, ControllerLayout, and B button here so they vanish before the wipe!")]
    public GameObject[] objectsToHideOnExit;

    [Header("When can player press Back/Switch?")]
    public bool armOnStart = false;
    public float delayBeforeInput = 0.8f;

    [Header("--- NEW: Image Swapping ---")]
    [Tooltip("Drag the Image component from the ControllerLayout object here")]
    public Image controllerLayoutImage;
    [Tooltip("Add your 3 layout images here (Xbox, PS, Keyboard)")]
    public Sprite[] layoutImages;
    private int currentLayoutIndex = 0;

    private bool armed = false;
    private bool fired = false;
    private bool canSwitch = true; // Prevents rapid-fire switching

    void Start()
    {
        if (paperTransitionAnimator != null)
        {
            paperTransitionAnimator.gameObject.SetActive(false);
        }

        // Set the starting image
        if (controllerLayoutImage != null && layoutImages != null && layoutImages.Length > 0)
        {
            controllerLayoutImage.sprite = layoutImages[0];
        }

        if (armOnStart)
            StartCoroutine(ArmAfterDelay(delayBeforeInput));
    }

    IEnumerator ArmAfterDelay(float s)
    {
        yield return new WaitForSeconds(s);
        ArmInput();
    }

    public void ArmInput()
    {
        armed = true;
    }

    void Update()
    {
        if (!armed || fired) return;

        // --- NEW: Left/Right Image Swapping Logic ---
        if (canSwitch && layoutImages != null && layoutImages.Length > 1)
        {
            bool movedRight = false;
            bool movedLeft = false;

            // Check Keyboard (A/D or Arrows)
            if (Keyboard.current != null)
            {
                if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame) movedRight = true;
                if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame) movedLeft = true;
            }

            // Check Gamepad (D-pad or Left Stick)
            if (Gamepad.current != null)
            {
                Vector2 stick = Gamepad.current.leftStick.ReadValue();

                if (Gamepad.current.dpad.right.wasPressedThisFrame || stick.x > 0.5f) movedRight = true;
                if (Gamepad.current.dpad.left.wasPressedThisFrame || stick.x < -0.5f) movedLeft = true;
            }

            if (movedRight)
            {
                currentLayoutIndex = (currentLayoutIndex + 1) % layoutImages.Length;
                controllerLayoutImage.sprite = layoutImages[currentLayoutIndex];
                StartCoroutine(SwitchCooldown());
            }
            else if (movedLeft)
            {
                currentLayoutIndex--;
                if (currentLayoutIndex < 0) currentLayoutIndex = layoutImages.Length - 1;
                controllerLayoutImage.sprite = layoutImages[currentLayoutIndex];
                StartCoroutine(SwitchCooldown());
            }
        }
        // --------------------------------------------

        if (BackPressed())
        {
            fired = true;
            StartCoroutine(ExitSequence());
        }
    }

    // Prevents the stick from instantly flying through all 3 images in one frame
    IEnumerator SwitchCooldown()
    {
        canSwitch = false;
        yield return new WaitForSeconds(0.2f);
        canSwitch = true;
    }

    IEnumerator ExitSequence()
    {
        if (bButtonAnimator != null)
        {
            bButtonAnimator.SetTrigger(bButtonTrigger);
        }

        yield return new WaitForSeconds(delayAfterBPressed);

        // Hide all the UI junk so the screen is clean for the wipe!
        if (objectsToHideOnExit != null)
        {
            foreach (GameObject obj in objectsToHideOnExit)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        if (paperTransitionAnimator != null)
        {
            paperTransitionAnimator.gameObject.SetActive(true);

            Image paperImg = paperTransitionAnimator.GetComponent<Image>();
            if (paperImg != null)
            {
                Color c = paperImg.color;
                c.a = 1f;
                paperImg.color = c;
            }

            if (!string.IsNullOrEmpty(paperReverseTriggerName))
            {
                paperTransitionAnimator.SetTrigger(paperReverseTriggerName);
            }

            paperTransitionAnimator.Update(0f);

            yield return new WaitForSeconds(paperAnimationDuration);
        }

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadSceneAsync(sceneToLoad);
        }
    }

    bool BackPressed()
    {
        if (Keyboard.current != null &&
           (Keyboard.current.escapeKey.wasPressedThisFrame ||
            Keyboard.current.backspaceKey.wasPressedThisFrame ||
            Keyboard.current.eKey.wasPressedThisFrame))
            return true;

        var pad = Gamepad.current;
        if (pad != null)
            return pad.buttonEast.wasPressedThisFrame ||
                   pad.startButton.wasPressedThisFrame ||
                   pad.selectButton.wasPressedThisFrame;

        return false;
    }
}