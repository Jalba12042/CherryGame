using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public enum InputMode { Keyboard, Gamepad, Arcade }

    public static InputMode CurrentMode
    {
        get
        {
            if (GameManager.Instance.isOnKeyboard)
                return InputMode.Keyboard;
            if (Gamepad.all.Count > 0)
                return InputMode.Gamepad;
            return InputMode.Arcade;
        }
    }

    private Gamepad[] assignedGamepads = new Gamepad[4];
    private bool[] assignedKeyboard = new bool[4];
    private bool[] hasExplicitAssignment = new bool[4];
    private int[] disconnectedDeviceIds = { -1, -1, -1, -1 };

    public event System.Action<int> OnPlayerDisconnected;
    public event System.Action<int> OnPlayerReconnected;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable() { InputSystem.onDeviceChange += OnDeviceChange; }
    void OnDisable() { InputSystem.onDeviceChange -= OnDeviceChange; }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (!(device is Gamepad gamepad)) return;

        if (change == InputDeviceChange.Disconnected)
        {
            for (int i = 0; i < assignedGamepads.Length; i++)
            {
                if (assignedGamepads[i] == gamepad)
                {
                    disconnectedDeviceIds[i] = device.deviceId;
                    assignedGamepads[i] = null;
                    OnPlayerDisconnected?.Invoke(i + 1);
                    break;
                }
            }
        }
        else if (change == InputDeviceChange.Reconnected || change == InputDeviceChange.Added)
        {
            for (int i = 0; i < disconnectedDeviceIds.Length; i++)
            {
                if (disconnectedDeviceIds[i] == device.deviceId)
                {
                    assignedGamepads[i] = gamepad;
                    disconnectedDeviceIds[i] = -1;
                    OnPlayerReconnected?.Invoke(i + 1);
                    break;
                }
            }
        }
    }

    public void AssignGamepad(int playerID, Gamepad pad)
    {
        if (playerID < 1 || playerID > 4) return;
        int idx = playerID - 1;
        assignedGamepads[idx] = pad;
        assignedKeyboard[idx] = false;
        hasExplicitAssignment[idx] = true;
        disconnectedDeviceIds[idx] = -1;
    }

    public void AssignKeyboard(int playerID)
    {
        if (playerID < 1 || playerID > 4) return;
        int idx = playerID - 1;
        assignedKeyboard[idx] = true;
        assignedGamepads[idx] = null;
        hasExplicitAssignment[idx] = true;
        disconnectedDeviceIds[idx] = -1;
    }

    public void UnassignGamepad(int playerID)
    {
        if (playerID < 1 || playerID > 4) return;
        int idx = playerID - 1;
        assignedGamepads[idx] = null;
        assignedKeyboard[idx] = false;
        hasExplicitAssignment[idx] = false;
        disconnectedDeviceIds[idx] = -1;
    }

    public bool IsPlayerDisconnected(int playerID)
    {
        if (playerID < 1 || playerID > 4) return false;
        int idx = playerID - 1;
        return hasExplicitAssignment[idx] && assignedGamepads[idx] == null && !assignedKeyboard[idx];
    }

    public bool IsKeyboardPlayer(int playerID)
    {
        if (playerID < 1 || playerID > 4) return false;
        return assignedKeyboard[playerID - 1];
    }

    public bool IsAssigned(int playerID)
    {
        if (playerID < 1 || playerID > 4) return false;
        int idx = playerID - 1;

        if (CurrentMode == InputMode.Arcade) return false;

        return assignedKeyboard[idx] || assignedGamepads[idx] != null;
    }

    // --- DYNAMIC UNASSIGNED KEYBOARD CHECKS ---
    public bool GetUnassignedKeyboardJoin()
    {
        // 1. Check every single player slot (0 through 3)
        // 2. If the keyboard is ALREADY assigned to ANY slot, instantly return FALSE and ignore the input!
        for (int i = 0; i < 4; i++)
            if (assignedKeyboard[i]) return false;

        // 3. If no keyboard is found in the lobby, THEN listen for the Spacebar or Enter key to join
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
    }

    public bool GetUnassignedKeyboardBack()
    {
        for (int i = 0; i < 4; i++) if (assignedKeyboard[i]) return false;
        return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E);
    }

    // -- Move -----------------------------------------------------

    public Vector2 GetMove(int playerID)
    {
        if (!IsAssigned(playerID)) return Vector2.zero;
        if (CurrentMode == InputMode.Arcade) return Vector2.zero;

        if (IsKeyboardPlayer(playerID)) return GetKeyboardMoveVector();

        return GetPad(playerID)?.leftStick.ReadValue() ?? Vector2.zero;
    }

    private Vector2 GetKeyboardMoveVector()
    {
        Vector2 dir = Vector2.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) dir.y += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) dir.y -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) dir.x += 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) dir.x -= 1f;
        return dir.normalized;
    }

    // -- Buttons (pressed this frame) -----------------------------

    public bool GetGrabDown(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        if (CurrentMode == InputMode.Arcade) return false;

        if (IsKeyboardPlayer(playerID)) return Input.GetKeyDown(KeyCode.Q);
        return GetPad(playerID)?.rightTrigger.wasPressedThisFrame ?? false;
    }

    public bool GetThrowDown(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        if (CurrentMode == InputMode.Arcade) return false;

        if (IsKeyboardPlayer(playerID)) return Input.GetKeyDown(KeyCode.T);
        return GetPad(playerID)?.leftTrigger.wasPressedThisFrame ?? false;
    }

    public bool GetDashDown(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        if (CurrentMode == InputMode.Arcade) return false;

        if (IsKeyboardPlayer(playerID)) return Input.GetKeyDown(KeyCode.LeftShift);
        return GetPad(playerID)?.buttonWest.wasPressedThisFrame ?? false;
    }

    public bool GetJumpDown(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        if (CurrentMode == InputMode.Arcade) return false;

        if (IsKeyboardPlayer(playerID)) return Input.GetKeyDown(KeyCode.Space);
        return GetPad(playerID)?.buttonSouth.wasPressedThisFrame ?? false;
    }

    public bool GetScreamDown(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        if (CurrentMode == InputMode.Arcade) return false;

        if (IsKeyboardPlayer(playerID)) return Input.GetKeyDown(KeyCode.R);
        return GetPad(playerID)?.buttonEast.wasPressedThisFrame ?? false;
    }

    public bool GetEscapeDown(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        if (CurrentMode == InputMode.Arcade) return false;

        if (IsKeyboardPlayer(playerID)) return Input.GetKeyDown(KeyCode.Escape);
        return GetPad(playerID)?.buttonNorth.wasPressedThisFrame ?? false;
    }

    // -- Buttons (held) -------------------------------------------

    public bool GetButton1Held(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        if (CurrentMode == InputMode.Arcade) return false;

        if (IsKeyboardPlayer(playerID)) return Input.GetKey(KeyCode.Q);
        return GetPad(playerID)?.rightTrigger.isPressed ?? false;
    }

    public bool GetButton2Held(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        if (CurrentMode == InputMode.Arcade) return false;

        if (IsKeyboardPlayer(playerID)) return Input.GetKey(KeyCode.T);
        return GetPad(playerID)?.leftTrigger.isPressed ?? false;
    }

    public bool GetButton3Held(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        if (CurrentMode == InputMode.Arcade) return false;

        if (IsKeyboardPlayer(playerID)) return Input.GetKey(KeyCode.LeftShift);
        return GetPad(playerID)?.buttonWest.isPressed ?? false;
    }

    // -- Menu -----------------------------------------------------

    public bool GetConfirmDown(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        if (CurrentMode == InputMode.Arcade) return false;

        if (IsKeyboardPlayer(playerID))
            return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);

        return GetPad(playerID)?.buttonSouth.wasPressedThisFrame ?? false;
    }

    public bool GetBackDown(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        if (CurrentMode == InputMode.Arcade) return false;

        if (IsKeyboardPlayer(playerID))
            return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E);

        return GetPad(playerID)?.buttonEast.wasPressedThisFrame ?? false;
    }

    public bool GetPauseDown(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        if (CurrentMode == InputMode.Arcade) return false;

        if (IsKeyboardPlayer(playerID)) return Input.GetKeyDown(KeyCode.Escape);
        return GetPad(playerID)?.startButton.wasPressedThisFrame ?? false;
    }

    // -- Menu (any device at once) ---------------------------------

    public float GetMenuMoveX()
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) return -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) return 1f;

        foreach (var pad in Gamepad.all)
        {
            float stickX = pad.leftStick.ReadValue().x;
            float dpadX = pad.dpad.ReadValue().x;
            float padX = Mathf.Abs(stickX) > Mathf.Abs(dpadX) ? stickX : dpadX;
            if (Mathf.Abs(padX) > 0.01f) return padX;
        }
        return 0f;
    }

    public bool GetMenuConfirmDown()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            return true;

        foreach (var pad in Gamepad.all)
            if (pad.buttonSouth.wasPressedThisFrame) return true;

        return false;
    }

    public bool GetMenuBackDown()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)) return true;

        foreach (var pad in Gamepad.all)
            if (pad.buttonEast.wasPressedThisFrame) return true;

        return false;
    }

    // -- Helper ---------------------------------------------------

    private Gamepad GetPad(int playerID)
    {
        int idx = playerID - 1;
        if (idx < 0 || idx >= assignedGamepads.Length) return null;

        return assignedGamepads[idx];
    }

    public Gamepad GetAssignedGamepad(int playerID)
    {
        return GetPad(playerID);
    }
}