using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LocalMenuController : MonoBehaviour
{
    [Header("Menu Buttons (2P, 3P, 4P in order)")]
    public LocalSelectable[] buttons;   // <- uses your LocalSelectable script

    [Header("Next Scene")]
    [SerializeField] private string connectSceneName = "ControllerConnectScene";

    private int currentIndex = 0;
    private bool canMove = true;
    private float deadzone = 0.5f;

    void Start()
    {
        HighlightCurrent();
    }

    void Update()
    {
        // Require a gamepad
        if (Gamepad.all.Count == 0) return;

        var gamepad = Gamepad.all[0];

        // Read left stick X (same logic you had)
        Vector2 move = gamepad.leftStick.ReadValue();

        if (canMove)
        {
            if (move.x > deadzone)
            {
                currentIndex = Mathf.Min(buttons.Length - 1, currentIndex + 1);
                HighlightCurrent();
                canMove = false;
            }
            else if (move.x < -deadzone)
            {
                currentIndex = Mathf.Max(0, currentIndex - 1);
                HighlightCurrent();
                canMove = false;
            }
        }

        // Let stick return to center before moving again
        if (Mathf.Abs(move.x) < 0.2f)
            canMove = true;

        // A / South = select
        if (gamepad.buttonSouth.wasPressedThisFrame)
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
}