using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))] // Ensures the object has an AudioSource
public class MainMenuController : MonoBehaviour
{
    [Header("Menu Buttons (Local, Multiplayer in this order)")]
    public MenuSelectable[] buttons;   // <<� make sure this says MenuSelectable[]

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
        // Optional: auto-find if you don�t want to drag
        if (buttons == null || buttons.Length == 0)
        {
            buttons = FindObjectsByType<MenuSelectable>(FindObjectsSortMode.None)
                      .OrderBy(b => b.transform.GetSiblingIndex())
                      .ToArray();
        }

        // Get the AudioSource to play our clips
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false; // Prevent a random sound at start
    }

    void Start() => HighlightCurrent();

    void Update()
    {
        if (InputManager.Instance == null || buttons == null || buttons.Length == 0) return;

        const int menuPlayer = 1;
        float xInput = InputManager.Instance.GetMove(menuPlayer).x;

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
        if (Mathf.Abs(xInput) < 0.2f) canMove = true;

        if (InputManager.Instance.GetConfirmDown(menuPlayer))
        {
            PlaySound(selectSound);
            buttons[currentIndex].Activate();
        }

        if (InputManager.Instance.GetBackDown(menuPlayer))
        {
            PlaySound(backSound);
        }
    }

    void HighlightCurrent()
    {
        for (int i = 0; i < buttons.Length; i++)
            buttons[i].Highlight(i == currentIndex);
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