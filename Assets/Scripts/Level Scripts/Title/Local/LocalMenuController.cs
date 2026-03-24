using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))] // Ensures the object has an AudioSource
public class LocalMenuController : MonoBehaviour
{
    [Header("Menu Buttons (2P, 3P, 4P in order)")]
    public LocalSelectable[] buttons;   // <- uses your LocalSelectable script

    [Header("Next Scene")]
    [SerializeField] private string connectSceneName = "ControllerConnectScene";

    [Header("UI Sound Effects")]
    public AudioClip navigateSound; // Left/Right Stick or D-Pad
    public AudioClip selectSound;   // A Button (South)
    public AudioClip backSound;     // B Button (East)

    private AudioSource audioSource;
    private int currentIndex = 0;
    private bool canMove = true;
    private float deadzone = 0.5f;

    void Awake()
    {
        // Get the AudioSource to play our clips
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        HighlightCurrent();
    }

    void Update()
    {
        // Require a gamepad
        if (Gamepad.all.Count == 0 || buttons == null || buttons.Length == 0) return;

        var gamepad = Gamepad.all[0];

        // Read left stick X or D-Pad (added D-Pad for consistency with your other script)
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

        // Let stick return to center before moving again
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
            // Assuming you want to go back to the Title/Main Menu. Change string as needed!
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

    // 0 -> 2P, 1 -> 3P, 2 -> 4P; then load connect scene
    void SelectOption(int index)
    {
        int players = index + 2;

        if (GameManager.Instance != null)
            GameManager.Instance.playerCount = players;
        else
            Debug.LogWarning("GameManager.Instance is null. Ensure it exists before loading the next scene.");

        if (!string.IsNullOrEmpty(connectSceneName))
            SceneManager.LoadScene(connectSceneName);
        else
            Debug.LogError("LocalMenuController: connectSceneName is empty.");
    }

    // Helper method to play sounds safely
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}