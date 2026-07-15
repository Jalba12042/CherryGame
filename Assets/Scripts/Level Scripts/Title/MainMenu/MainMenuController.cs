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
        if (Mathf.Abs(xInput) < 0.2f) canMove = true;

        if (InputManager.Instance.GetMenuConfirmDown())
            ConfirmIndex(currentIndex);

        if (InputManager.Instance.GetMenuBackDown())
        {
            PlaySound(backSound);
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
        if (buttons == null || index < 0 || index >= buttons.Length) return;
        currentIndex = index;
        HighlightCurrent();
        PlaySound(selectSound);
        buttons[currentIndex].Activate();
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