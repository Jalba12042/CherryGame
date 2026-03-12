using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJoinController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject[] joinPanels;   // Drag P1, P2, P3, P4 join panels
    public GameObject[] menuPanels;   // Drag P1, P2, P3, P4 menu panels

    [Header("Ready UI")]
    public GameObject[] readyPanels;   // Drag P1Ready, P2Ready, etc.

    private bool[] isReady;

    private Gamepad[] controllers;
    private int[] assignedControllers;   // -1 = empty, otherwise controller index

    void Start()
    {
        controllers = Gamepad.all.ToArray();
        assignedControllers = new int[joinPanels.Length];

        controllers = Gamepad.all.ToArray();
        Debug.Log("Detected controllers: " + controllers.Length);

        for (int i = 0; i < controllers.Length; i++)
            Debug.Log($"Controller {i}: {controllers[i].displayName}");

        for (int i = 0; i < assignedControllers.Length; i++)
        {
            assignedControllers[i] = -1;
            joinPanels[i].SetActive(true);
            menuPanels[i].SetActive(false);
        }

        // Make sure GameManager's array matches this screen
        if (GameManager.Instance != null)
        {
            GameManager.Instance.controllerAssignments = new int[joinPanels.Length];
            for (int i = 0; i < joinPanels.Length; i++)
                GameManager.Instance.controllerAssignments[i] = -1;
        }

        isReady = new bool[joinPanels.Length];

        for (int i = 0; i < isReady.Length; i++)
        {
            isReady[i] = false;
            readyPanels[i].SetActive(false);
        }

    }


    void Update()
    {
        for (int c = 0; c < controllers.Length; c++)
        {
            Gamepad pad = controllers[c];
            if (pad == null) continue;

            if (pad.buttonSouth.wasPressedThisFrame)
            {
                Debug.Log($"A pressed on controller index {c}");

                // If this controller already belongs to a player, check ready
                int playerIndex = GetPlayerIndexFromController(c);
                if (playerIndex != -1)
                {
                    HandleReadyPress(playerIndex);
                    continue;
                }

                // Otherwise try to join
                TryAssignController(c);
            }
        }
    }


    void TryAssignController(int controllerIndex)
    {
        // Prevent the same controller from joining twice
        for (int i = 0; i < assignedControllers.Length; i++)
        {
            if (assignedControllers[i] == controllerIndex)
                return;
        }

        // Assign controller to the first empty player slot
        for (int player = 0; player < assignedControllers.Length; player++)
        {
            // SAFETY CHECK: prevent out-of-range UI access
            if (player >= joinPanels.Length || player >= menuPanels.Length)
                return;

            if (assignedControllers[player] == -1)
            {
                assignedControllers[player] = controllerIndex;

                joinPanels[player].SetActive(false);
                menuPanels[player].SetActive(true);

                if (GameManager.Instance != null)
                    GameManager.Instance.controllerAssignments[player] = controllerIndex;

                Debug.Log($"Controller {controllerIndex} joined as Player {player + 1}");
                return;
            }
        }

        // All slots full → ignore extra controllers
    }

    int GetPlayerIndexFromController(int controllerIndex)
    {
        for (int i = 0; i < assignedControllers.Length; i++)
        {
            if (assignedControllers[i] == controllerIndex)
                return i;
        }
        return -1;
    }

    void HandleReadyPress(int player)
    {
        // Only allow ready if menu panel is active
        if (!menuPanels[player].activeSelf)
            return;

        if (!isReady[player])
        {
            isReady[player] = true;
            readyPanels[player].SetActive(true);
            Debug.Log($"Player {player + 1} is READY");
        }
        else
        {
            // Optional: allow un-ready
            isReady[player] = false;
            readyPanels[player].SetActive(false);
            Debug.Log($"Player {player + 1} is NOT ready");
        }
    }


}
