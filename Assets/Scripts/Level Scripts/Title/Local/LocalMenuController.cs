using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class LocalMenuController : MonoBehaviour
{
    [Header("Menu Buttons (2P, 3P, 4P, and BACK in order)")]
    public LocalSelectable[] buttons;

    [Header("Scenes")]
    [SerializeField] private string connectSceneName = "ControllerConnectScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("UI Sound Effects")]
    public AudioClip navigateSound;
    public AudioClip selectSound;
    public AudioClip backSound;

    [Header("Transition Settings")]
    public MenuExitOrchestrator exitOrchestrator; // Points to your AnimationManager
    public GameObject[] puppetsToHide;            // Drop your animated characters here!

    private AudioSource audioSource;
    private int currentIndex = 0;
    private bool canMove = true;
    private float deadzone = 0.5f;
    private bool isTransitioning = false;

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
            StartExitSequence(mainMenuSceneName);
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
        if (isTransitioning) return;

        // Check if they clicked the BACK Button
        if (index == buttons.Length - 1)
        {
            StartExitSequence(mainMenuSceneName);
            return;
        }

        // Otherwise, it's a player count button (0 = 2P, 1 = 3P, 2 = 4P)
        int players = index + 2;

        if (GameManager.Instance != null)
            GameManager.Instance.playerCount = players;

        StartExitSequence(connectSceneName);
    }

    // --- THE MAGIC TRICK ---
    private void StartExitSequence(string targetScene)
    {
        if (isTransitioning) return;
        isTransitioning = true; // Lock controls

        // 1. INSTANTLY hide the puppets without any animation
        if (puppetsToHide != null)
        {
            foreach (GameObject puppet in puppetsToHide)
            {
                if (puppet != null) puppet.SetActive(false);
            }
        }

        // 2. Tell the Orchestrator to drop the signs/hill and load the scene
        if (exitOrchestrator != null)
        {
            exitOrchestrator.ExitThenLoad(targetScene);
        }
        else
        {
            // Fallback just in case
            SceneManager.LoadScene(targetScene);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}