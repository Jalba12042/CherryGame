using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public enum InputMode { Keyboard, Gamepad, Arcade }

    // Evaluates live every time it's accessed � no stale state
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
    private bool[] hasExplicitAssignment = new bool[4];
    private int[] disconnectedDeviceIds = { -1, -1, -1, -1 };

    public event System.Action<int> OnPlayerDisconnected;
    public event System.Action<int> OnPlayerReconnected;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

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
        hasExplicitAssignment[idx] = true;
        disconnectedDeviceIds[idx] = -1;
    }

    public void UnassignGamepad(int playerID)
    {
        if (playerID < 1 || playerID > 4) return;
        int idx = playerID - 1;
        assignedGamepads[idx] = null;
        hasExplicitAssignment[idx] = false;
        disconnectedDeviceIds[idx] = -1;
    }

    public bool IsPlayerDisconnected(int playerID)
    {
        if (playerID < 1 || playerID > 4) return false;
        int idx = playerID - 1;
        return hasExplicitAssignment[idx] && assignedGamepads[idx] == null;
    }

    public bool IsAssigned(int playerID)
    {
        if (playerID < 1 || playerID > 4) return false;
        int idx = playerID - 1;
        return CurrentMode switch
        {
            InputMode.Keyboard => playerID == 1,
            // If a player had an explicit assignment but disconnected, don't fall back to another pad
            InputMode.Gamepad => assignedGamepads[idx] != null || (!hasExplicitAssignment[idx] && idx < Gamepad.all.Count),
            InputMode.Arcade => /*true*/ false,
            _ => false
        };
    }

    // ?? Move ?????????????????????????????????????????????????????

    public Vector2 GetMove(int playerID)
    {
        if (!IsAssigned(playerID)) return Vector2.zero;
        string prefix = "P" + playerID;

        return CurrentMode switch
        {
            InputMode.Keyboard => new Vector2(Input.GetAxis("HorizontalWASD"), Input.GetAxis("VerticalWASD")),
            InputMode.Gamepad => GetPad(playerID)?.leftStick.ReadValue() ?? Vector2.zero,
            InputMode.Arcade => /*new Vector2(Input.GetAxisRaw(prefix + "Horizontal"), Input.GetAxisRaw(prefix + "Vertical"))*/ Vector2.zero,
            _ => Vector2.zero
        };
    }

    // ?? Buttons (pressed this frame) ?????????????????????????????

    public bool GetGrabDown(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        string prefix = "P" + playerID;

        return CurrentMode switch
        {
            InputMode.Keyboard => Input.GetKeyDown(KeyCode.E),
            InputMode.Gamepad => GetPad(playerID)?.rightTrigger.wasPressedThisFrame ?? false,
            InputMode.Arcade => /*Input.GetButtonDown(prefix + "Button1")*/ false,
            _ => false
        };
    }

    public bool GetThrowDown(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        string prefix = "P" + playerID;

        return CurrentMode switch
        {
            InputMode.Keyboard => Input.GetKeyDown(KeyCode.T),
            InputMode.Gamepad => GetPad(playerID)?.leftTrigger.wasPressedThisFrame ?? false,
            InputMode.Arcade => /*Input.GetButtonDown(prefix + "Button2")*/ false,
            _ => false
        };
    }

    public bool GetDashDown(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        string prefix = "P" + playerID;

        return CurrentMode switch
        {
            InputMode.Keyboard => Input.GetKeyDown(KeyCode.LeftShift),
            InputMode.Gamepad => GetPad(playerID)?.buttonWest.wasPressedThisFrame ?? false,
            InputMode.Arcade => /*Input.GetButtonDown(prefix + "Button3")*/ false,
            _ => false
        };
    }

    public bool GetJumpDown(int playerID)
    {
        if (!IsAssigned(playerID)) return false;

        return CurrentMode switch
        {
            InputMode.Keyboard => Input.GetKeyDown(KeyCode.Space),
            InputMode.Gamepad => GetPad(playerID)?.buttonSouth.wasPressedThisFrame ?? false,
            InputMode.Arcade => false,
            _ => false
        };
    }

    public bool GetScreamDown(int playerID)
    {
        if (!IsAssigned(playerID)) return false;

        return CurrentMode switch
        {
            InputMode.Keyboard => Input.GetKeyDown(KeyCode.R),
            InputMode.Gamepad => GetPad(playerID)?.buttonEast.wasPressedThisFrame ?? false,
            InputMode.Arcade => false,
            _ => false
        };
    }

    public bool GetEscapeDown(int playerID)
    {
        if (!IsAssigned(playerID)) return false;

        return CurrentMode switch
        {
            InputMode.Keyboard => Input.GetKeyDown(KeyCode.Q),
            InputMode.Gamepad => GetPad(playerID)?.buttonNorth.wasPressedThisFrame ?? false,
            InputMode.Arcade => false,
            _ => false
        };
    }

    // ?? Buttons (held) ???????????????????????????????????????????

    public bool GetButton1Held(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        string prefix = "P" + playerID;

        return CurrentMode switch
        {
            InputMode.Keyboard => Input.GetKey(KeyCode.E),
            InputMode.Gamepad => GetPad(playerID)?.rightTrigger.isPressed ?? false,
            InputMode.Arcade => /*Input.GetButton(prefix + "Button1")*/ false,
            _ => false
        };
    }

    public bool GetButton2Held(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        string prefix = "P" + playerID;

        return CurrentMode switch
        {
            InputMode.Keyboard => Input.GetKey(KeyCode.T),
            InputMode.Gamepad => GetPad(playerID)?.leftTrigger.isPressed ?? false,
            InputMode.Arcade => /*Input.GetButton(prefix + "Button2")*/ false,
            _ => false
        };
    }

    public bool GetButton3Held(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        string prefix = "P" + playerID;

        return CurrentMode switch
        {
            InputMode.Keyboard => Input.GetKey(KeyCode.LeftShift),
            InputMode.Gamepad => GetPad(playerID)?.buttonWest.isPressed ?? false,
            InputMode.Arcade => /*Input.GetButton(prefix + "Button3")*/ false,
            _ => false
        };
    }

    // ?? Menu ?????????????????????????????????????????????????????

    public bool GetConfirmDown(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        string prefix = "P" + playerID;

        return CurrentMode switch
        {
            InputMode.Keyboard => Input.GetKeyDown(KeyCode.Return),
            InputMode.Gamepad => GetPad(playerID)?.buttonSouth.wasPressedThisFrame ?? false,
            InputMode.Arcade => /*Input.GetButtonDown(prefix + "Button1")*/ false,
            _ => false
        };
    }

    public bool GetBackDown(int playerID)
    {
        if (!IsAssigned(playerID)) return false;
        string prefix = "P" + playerID;

        return CurrentMode switch
        {
            InputMode.Keyboard => Input.GetKeyDown(KeyCode.Escape),
            InputMode.Gamepad => GetPad(playerID)?.buttonEast.wasPressedThisFrame ?? false,
            InputMode.Arcade => /*Input.GetButtonDown(prefix + "Button2")*/ false,
            _ => false
        };
    }

    // ?? Pause ????????????????????????????????????????????????????

    public bool GetPauseDown(int playerID)
    {
        if (!IsAssigned(playerID)) return false;

        return CurrentMode switch
        {
            InputMode.Keyboard => Input.GetKeyDown(KeyCode.Escape),
            InputMode.Gamepad => GetPad(playerID)?.startButton.wasPressedThisFrame ?? false,
            InputMode.Arcade => /*Input.GetKeyDown(KeyCode.Space)*/ false,
            _ => false
        };
    }

    // ?? Helper ???????????????????????????????????????????????????

    private Gamepad GetPad(int playerID)
    {
        int idx = playerID - 1;
        if (idx < 0 || idx >= assignedGamepads.Length) return null;
        if (assignedGamepads[idx] != null) return assignedGamepads[idx];
        if (!hasExplicitAssignment[idx] && idx < Gamepad.all.Count) return Gamepad.all[idx];
        return null;
    }
}