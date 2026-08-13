using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class MainMenuController : MonoBehaviour
{
    [Header("Menu Buttons")]
    public MenuSelectable[] buttons;

    [Header("Transition Link")]
    [Tooltip("Drag MultiplayerMan here so the B button works!")]
    public MultiplayerBackTransition backTransition; // <-- NEW VARIABLE!

    [Header("UI Sound Effects")]
    public AudioClip navigateSound;
    public AudioClip selectSound;
    public AudioClip backSound;

    private AudioSource audioSource;
    private int currentIndex = 0;
    private bool canMove = true;
    private float deadzone = 0.5f;

    void Awake()
    {
        if (buttons == null || buttons.Length == 0)
        {
            buttons = FindObjectsByType<MenuSelectable>(FindObjectsSortMode.None)
                      .OrderBy(b => b.transform.GetSiblingIndex())
                      .ToArray();
        }
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
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

            // THE FIX: Actually play the transition when B/Escape is pressed!
            if (backTransition != null)
            {
                backTransition.PlayBackTransition();
            }
        }

        HandleMouse();
    }

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

    public void SelectIndex(int index)
    {
        if (buttons == null || index < 0 || index >= buttons.Length || index == currentIndex) return;
        currentIndex = index;
        HighlightCurrent();
        PlaySound(navigateSound);
    }

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

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}