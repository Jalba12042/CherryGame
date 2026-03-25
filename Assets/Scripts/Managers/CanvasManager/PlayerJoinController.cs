using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerJoinController : MonoBehaviour
{
    /*[Header("UI Panels")]
    public GameObject[] joinPanels;   // Drag P1, P2, P3, P4 join panels
    public GameObject[] menuPanels;   // Drag P1, P2, P3, P4 menu panels

    [Header("Ready UI")]
    public GameObject[] readyPanels;   // Drag P1Ready, P2Ready, etc.

    [Header("Player Preview Cameras")]
    public Camera[] previewCameras; // Drag PlayerPreviewCam1, Cam2, etc.

    [Header("Player Preview UI")]
    public RawImage[] previewImages; // Drag PreviewRawImage1, RawImage2, etc.*/

    public PlayerSlots[] slots;
    public GameObject playerModelPrefab; // add this too



    private bool[] isReady;

    private Gamepad[] controllers;
    private int[] assignedControllers;   // -1 = empty, otherwise controller index

    void Start()
    {
        // Detect controllers
        controllers = Gamepad.all.ToArray();
        Debug.Log("Detected controllers: " + controllers.Length);

        for (int i = 0; i < controllers.Length; i++)
            Debug.Log($"Controller {i}: {controllers[i].displayName}");

        // Setup controller assignment array
        assignedControllers = new int[slots.Length];
        isReady = new bool[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            assignedControllers[i] = -1;
            isReady[i] = false;

            // UI defaults
            slots[i].joinPanel.SetActive(true);
            slots[i].menuPanel.SetActive(false);
            slots[i].readyPanel.SetActive(false);

            // Preview defaults
            if (slots[i].previewCamera != null)
                slots[i].previewCamera.gameObject.SetActive(false);

            if (slots[i].previewImage != null)
                slots[i].previewImage.gameObject.SetActive(false);

            // No model spawned yet
            slots[i].spawnedModel = null;
        }

        // Sync with GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.controllerAssignments = new int[slots.Length];

            for (int i = 0; i < slots.Length; i++)
                GameManager.Instance.controllerAssignments[i] = -1;
        }
    }


    /*void Start()
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

    }*/


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


            if (pad.buttonEast.wasPressedThisFrame) // B button
            {
                int playerIndex = GetPlayerIndexFromController(c);
                if (playerIndex != -1)
                {
                    HandleBackPress(playerIndex);
                }
            }

            // CUSTOMIZATION NAVIGATION
            for (int p = 0; p < slots.Length; p++)
            {
                if (assignedControllers[p] != c)
                    continue; // this controller does not control this slot

                var ui = slots[p].customizationUI;
                if (ui == null)
                    continue;

                Vector2 stick = pad.leftStick.ReadValue();

                // Move UP
                if (stick.y > 0.5f && !slots[p].stickInUse)
                {
                    ui.MoveSelection(-1);
                    slots[p].stickInUse = true;
                }
                // Move DOWN
                else if (stick.y < -0.5f && !slots[p].stickInUse)
                {
                    ui.MoveSelection(1);
                    slots[p].stickInUse = true;
                }

                // Reset stick lock
                if (Mathf.Abs(stick.y) < 0.2f)
                    slots[p].stickInUse = false;

                // ===== RT / LT INPUT =====
                int category = ui.GetCurrentCategoryIndex();

                var customization = slots[p].spawnedModel.GetComponentInChildren<PlayerCustomization>();

                if (pad.rightTrigger.wasPressedThisFrame)
                {
                    if (category == 0)
                    {
                        ui.ChangeColor(1, slots[p].spawnedModel, p);
                    }
                    else if (category == 1)
                    {
                        customization.ChangeHead(1);
                    }
                    else if (category == 2)
                    {
                        customization.ChangeTorso(1);
                    }
                    else if (category == 3)
                    {
                        customization.ChangeBottom(1);
                    }
                }

                if (pad.leftTrigger.wasPressedThisFrame)
                {
                    if (category == 0)
                    {
                        ui.ChangeColor(-1, slots[p].spawnedModel, p);
                    }
                    else if (category == 1)
                    {
                        customization.ChangeHead(-1);
                    }
                    else if (category == 2)
                    {
                        customization.ChangeTorso(-1);
                    }
                    else if (category == 3)
                    {
                        customization.ChangeBottom(-1);
                    }
                }
            }
        }
    }


    void TryAssignController(int controllerIndex)
    {
        // Prevent duplicate joins
        for (int i = 0; i < assignedControllers.Length; i++)
        {
            if (assignedControllers[i] == controllerIndex)
                return;
        }

        for (int player = 0; player < assignedControllers.Length; player++)
        {
            if (assignedControllers[player] == -1)
            {
                assignedControllers[player] = controllerIndex;

                // UI
                slots[player].joinPanel.SetActive(false);
                slots[player].menuPanel.SetActive(true);
                slots[player].customizationUI.gameObject.SetActive(true);

                if (slots[player].customizationUI != null)
                {
                    slots[player].customizationUI.Initialize();
                }

                // Preview camera
                slots[player].previewCamera.gameObject.SetActive(true);
                slots[player].previewImage.texture = slots[player].previewCamera.targetTexture;
                slots[player].previewImage.gameObject.SetActive(true);

                // Spawn model
                slots[player].spawnedModel = Instantiate(
     playerModelPrefab,
     slots[player].modelSpawnPoint.position,
     slots[player].modelSpawnPoint.rotation,
     slots[player].modelSpawnPoint);

                var customization = slots[player].spawnedModel.GetComponentInChildren<PlayerCustomization>();

                if (customization != null)
                {
                    customization.Initialize();
                }

                // Set default color per player
                int defaultColorIndex = player;
                // Player 0 → index 0 (Blue)
                // Player 1 → index 1 (Red)

                slots[player].customizationUI.SetColorIndex(defaultColorIndex, slots[player].spawnedModel);

                var movement = slots[player].spawnedModel.GetComponent<Playermovement>();
                if (movement != null)
                    movement.enabled = false;



                // Save controller assignment
                if (GameManager.Instance != null)
                    GameManager.Instance.controllerAssignments[player] = controllerIndex;

                Debug.Log($"Controller {controllerIndex} joined as Player {player + 1}");
                return;
            }
        }
    }


    /*void TryAssignController(int controllerIndex)
    {
        for (int i = 0; i < assignedControllers.Length; i++)
        {
            if (assignedControllers[i] == controllerIndex)
                return;
        }

        for (int player = 0; player < assignedControllers.Length; player++)
        {
            if (player >= joinPanels.Length || player >= menuPanels.Length)
                return;

            if (assignedControllers[player] == -1)
            {
                assignedControllers[player] = controllerIndex;

                // Hide join UI
                joinPanels[player].SetActive(false);

                // Show menu UI
                menuPanels[player].SetActive(true);

                // Enable preview camera feed
                if (previewImages[player] != null && previewCameras[player] != null)
                {
                    previewImages[player].texture = previewCameras[player].targetTexture;
                    previewImages[player].gameObject.SetActive(true);
                    previewCameras[player].gameObject.SetActive(true);
                }

                if (GameManager.Instance != null)
                    GameManager.Instance.controllerAssignments[player] = controllerIndex;

                Debug.Log($"Controller {controllerIndex} joined as Player {player + 1}");
                return;
            }
        }
    }*/


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
        if (!slots[player].menuPanel.activeSelf)
            return;

        if (!isReady[player])
        {
            isReady[player] = true;
            slots[player].readyPanel.SetActive(true);

            // Save customization here later
        }
        else
        {
            isReady[player] = false;
            slots[player].readyPanel.SetActive(false);
        }
    }


    /*void HandleReadyPress(int player)
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
    }*/

    void HandleBackPress(int player)
    {
        // If ready → unready
        if (isReady[player])
        {
            isReady[player] = false;
            slots[player].readyPanel.SetActive(false);
            return;
        }

        // Remove player completely
        int controllerIndex = assignedControllers[player];
        assignedControllers[player] = -1;

        // UI reset
        slots[player].joinPanel.SetActive(true);
        slots[player].menuPanel.SetActive(false);
        slots[player].readyPanel.SetActive(false);

        // Turn off preview
        slots[player].previewCamera.gameObject.SetActive(false);
        slots[player].previewImage.gameObject.SetActive(false);

        // Destroy model
        if (slots[player].spawnedModel != null)
            Destroy(slots[player].spawnedModel);

        slots[player].spawnedModel = null;

        if (GameManager.Instance != null)
            GameManager.Instance.controllerAssignments[player] = -1;

        Debug.Log($"Player {player + 1} left (Controller {controllerIndex})");
    }

    public bool IsColorTaken(int colorIndex, int requestingPlayer)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i == requestingPlayer) continue;

            var ui = slots[i].customizationUI;
            if (ui == null) continue;

            if (ui.GetCurrentColorIndex() == colorIndex)
                return true;
        }

        return false;
    }

    /*void HandleBackPress(int player)
    {
        if (isReady[player])
        {
            isReady[player] = false;
            readyPanels[player].SetActive(false);
            return;
        }

        // Turn off preview
        if (previewImages[player] != null)
            previewImages[player].gameObject.SetActive(false);

        if (previewCameras[player] != null)
            previewCameras[player].gameObject.SetActive(false);

        assignedControllers[player] = -1;

        joinPanels[player].SetActive(true);
        menuPanels[player].SetActive(false);
        readyPanels[player].SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.controllerAssignments[player] = -1;

        Debug.Log($"Player {player + 1} left");
    }*/



}
