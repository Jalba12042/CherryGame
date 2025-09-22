using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ControllerConnect : MonoBehaviour
{
    public Image blueSlot;   // Blue box
    public Image redSlot;    // Red box
    public Image white1;     // Waiting square for Controller 1
    public Image white2;     // Waiting square for Controller 2
    public Button playButton;

    private Gamepad controller1;
    private Gamepad controller2;

    // Track where each controller is (0 = waiting, 1 = blue, 2 = red)
    private int controller1Pos = 0;
    private int controller2Pos = 0;

    void Start()
    {
        playButton.gameObject.SetActive(false);

        // Assign first two controllers if available
        if (Gamepad.all.Count > 0) controller1 = Gamepad.all[0];
        if (Gamepad.all.Count > 1) controller2 = Gamepad.all[1];

        ResetUI();
    }

    void Update()
    {
        if (controller1 != null) HandleController(controller1, ref controller1Pos, white1, 1);
        if (controller2 != null) HandleController(controller2, ref controller2Pos, white2, 2);

        // Enable Play only when both controllers are in DIFFERENT slots
        if ((controller1Pos == 1 || controller1Pos == 2) &&
            (controller2Pos == 1 || controller2Pos == 2) &&
            controller1Pos != controller2Pos)
        {
            playButton.gameObject.SetActive(true);
        }
        else
        {
            playButton.gameObject.SetActive(false);
        }
    }


    void HandleController(Gamepad gamepad, ref int pos, Image whiteSquare, int playerId)
    {
        Vector2 move = gamepad.leftStick.ReadValue();

        // --- From Waiting (pos == 0) ---
        if (pos == 0)
        {
            if (move.y > 0.5f && move.x < -0.5f && controller1Pos != 1 && controller2Pos != 1)
            {
                // Enter Blue slot
                pos = 1;
                blueSlot.color = Color.blue;
                whiteSquare.color = Color.gray;
                Debug.Log($"Player {playerId} entered BLUE slot");
            }
            else if (move.y > 0.5f && move.x > 0.5f && controller1Pos != 2 && controller2Pos != 2)
            {
                // Enter Red slot
                pos = 2;
                redSlot.color = Color.red;
                whiteSquare.color = Color.gray;
                Debug.Log($"Player {playerId} entered RED slot");
            }
        }

        // --- From Blue (pos == 1) ---
        else if (pos == 1)
        {
            if (move.x > 0.5f && controller1Pos != 2 && controller2Pos != 2)
            {
                // Switch to Red
                blueSlot.color = Color.gray;
                redSlot.color = Color.red;
                pos = 2;
                Debug.Log($"Player {playerId} switched to RED slot");
            }
            else if (move.y < -0.5f)
            {
                // Back to waiting
                blueSlot.color = Color.gray;
                pos = 0;
                whiteSquare.color = Color.white;
                Debug.Log($"Player {playerId} returned to waiting");
            }
        }

        // --- From Red (pos == 2) ---
        else if (pos == 2)
        {
            if (move.x < -0.5f && controller1Pos != 1 && controller2Pos != 1)
            {
                // Switch to Blue
                redSlot.color = Color.gray;
                blueSlot.color = Color.blue;
                pos = 1;
                Debug.Log($"Player {playerId} switched to BLUE slot");
            }
            else if (move.y < -0.5f)
            {
                // Back to waiting
                redSlot.color = Color.gray;
                pos = 0;
                whiteSquare.color = Color.white;
                Debug.Log($"Player {playerId} returned to waiting");
            }
        }
    }


    public void OnPlayPressed()
    {
        // Only controller1 (Player 1) can press A to start
        if (controller1 != null && controller1.buttonSouth.wasPressedThisFrame)
        {
            GameManager.Instance.playerCount = 2;
            SceneManager.LoadScene("GameScene");
        }
    }

    void ResetUI()
    {
        blueSlot.color = Color.gray;
        redSlot.color = Color.gray;
        white1.color = Color.white;
        white2.color = Color.white;
    }
}
