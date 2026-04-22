using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections; // NEW: We need this for the Coroutine!

[RequireComponent(typeof(AudioSource))]
public class LocalMenuController : MonoBehaviour
{
    [Header("Menu Buttons (2P, 3P, 4P in order)")]
    public LocalSelectable[] buttons;

    [Header("Next Scene")]
    [SerializeField] private string connectSceneName = "ControllerConnectScene";

    [Header("UI Sound Effects")]
    public AudioClip navigateSound;
    public AudioClip selectSound;
    public AudioClip backSound;

    // --- NEW: TRANSITION SETTINGS ---
    [Header("Transition Settings")]
    public Animator transitionAnimator;     // Drag your animation object here!
    public string transitionTrigger = "Exit"; // The name of the Trigger in your Animator
    public float transitionWaitTime = 1.0f;   // How long to wait before loading the scene

    private AudioSource audioSource;
    private int currentIndex = 0;
    private bool canMove = true;
    private float deadzone = 0.5f;
    private bool isTransitioning = false; // NEW: Locks the controls during the animation!

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        HighlightCurrent();
    }

    void Update()
    {
        // NEW: If the transition is playing, ignore all controller input!
        if (isTransitioning) return;

        if (Gamepad.all.Count == 0 || buttons == null || buttons.Length == 0) return;

        var gamepad = Gamepad.all[0];

        Vector2 move = gamepad.leftStick.ReadValue();
        float xInput = move.x;
        if (gamepad.dpad.left.wasPressedThisFrame) xInput = -1;
        if (gamepad.dpad.right.wasPressedThisFrame) xInput = 1;

        if (canMove)
        {
            if (xInput > deadzone)
            {
                int newIndex = Mathf.Min(buttons.Length - 1, currentIndex + 1);
                if (newIndex != currentIndex)
                {
                    currentIndex = newIndex;
                    HighlightCurrent();
                    PlaySound(navigateSound);
                }
                canMove = false;
            }
            else if (xInput < -deadzone)
            {
                int newIndex = Mathf.Max(0, currentIndex - 1);
                if (newIndex != currentIndex)
                {
                    currentIndex = newIndex;
                    HighlightCurrent();
                    PlaySound(navigateSound);
                }
                canMove = false;
            }
        }

        if (Mathf.Abs(xInput) < 0.2f)
            canMove = true;

        // A / South = select
        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            PlaySound(selectSound);
            SelectOption(currentIndex);
        }

        // B / East = back
        if (gamepad.buttonEast.wasPressedThisFrame)
        {
            PlaySound(backSound);
            SceneManager.LoadScene("MainMenu");
        }
    }

    void HighlightCurrent()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i == currentIndex) buttons[i].Highlight(true);
            else buttons[i].Highlight(false);
        }
    }

    void SelectOption(int index)
    {
        if (isTransitioning) return; // Double check so they don't trigger it twice

        int players = index + 2;

        if (GameManager.Instance != null)
            GameManager.Instance.playerCount = players;
        else
            Debug.LogWarning("GameManager.Instance is null. Ensure it exists before loading the next scene.");

        // --- NEW: START THE TRANSITION ANIMATION INSTEAD OF LOADING INSTANTLY ---
        StartCoroutine(TransitionToNextScene());
    }

    // --- THE MAGIC TRICK ---
    IEnumerator TransitionToNextScene()
    {
        isTransitioning = true; // Lock the controllers!

        // 1. Play the animation
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger(transitionTrigger);
        }

        // 2. Wait for the animation to finish
        yield return new WaitForSeconds(transitionWaitTime);

        // 3. Finally, load the Controller Connect scene!
        if (!string.IsNullOrEmpty(connectSceneName))
            SceneManager.LoadScene(connectSceneName);
        else
            Debug.LogError("LocalMenuController: connectSceneName is empty.");
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}