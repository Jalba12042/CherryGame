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

        if (buttons == null || buttons.Length == 0) return;

        float xInput = InputManager.Instance.GetMenuMoveX();

        if (canMove)
        {
            if (xInput > deadzone)
            {
                SelectIndex(Mathf.Min(buttons.Length - 1, currentIndex + 1));
                canMove = false;
            }
            else if (xInput < -deadzone)
            {
                SelectIndex(Mathf.Max(0, currentIndex - 1));
                canMove = false;
            }
        }

        if (Mathf.Abs(xInput) < 0.2f)
            canMove = true;

        // Confirm
        if (InputManager.Instance.GetMenuConfirmDown())
            ConfirmIndex(currentIndex);

        // Back
        if (InputManager.Instance.GetMenuBackDown())
        {
            PlaySound(backSound);
            StartExitSequence(mainMenuSceneName);
        }

        HandleMouse();
    }

    // Direct mouse hit-test instead of Unity's EventSystem/GraphicRaycaster pipeline —
    // the scene's InputSystemUIInputModule points at a stale package-sample actions asset
    // instead of the project's own, so IPointerEnterHandler/IPointerClickHandler are unreliable here.
    void HandleMouse()
    {
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        for (int i = 0; i < buttons.Length; i++)
        {
            RectTransform rt = buttons[i].RectTransform;
            if (rt == null || !RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos, null))
                continue;

            SelectIndex(i);
            if (Mouse.current.leftButton.wasPressedThisFrame)
                ConfirmIndex(i);
            break;
        }
    }

    // Moves the highlighted cursor to index (keyboard/gamepad nav, or a mouse hover)
    public void SelectIndex(int index)
    {
        if (buttons == null || index < 0 || index >= buttons.Length || index == currentIndex) return;
        currentIndex = index;
        HighlightCurrent();
        PlaySound(navigateSound);
    }

    // Activates index directly (keyboard/gamepad confirm, or a mouse click)
    public void ConfirmIndex(int index)
    {
        if (isTransitioning || buttons == null || index < 0 || index >= buttons.Length) return;
        currentIndex = index;
        HighlightCurrent();
        PlaySound(selectSound);
        SelectOption(currentIndex);
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
        {
            GameManager.Instance.playerCount = players;

            //Resets player score from previous game
            GameManager.Instance.playerTotalScores = new int[4];
            GameManager.Instance.playerCustomizations.Clear();
        }

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