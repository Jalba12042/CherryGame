using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerJoinController : MonoBehaviour
{
    public PlayerSlots[] slots;
    public GameObject playerModelPrefab;

    private bool[] isReady;
    private Gamepad[] controllers;
    private int[] assignedControllers;

    public GameObject countdownPanel;
    public TextMeshProUGUI countdownText;

    private bool countdownStarted = false;

    [Header("Intro Animation Settings")]
    public float introDelay = 2.0f; // Wait 120 frames (2 seconds)
    public float fadeDuration = 0.5f; // Fade in over 0.5 seconds
    private bool canInteract = false; // Locks the controllers during the intro

    [Header("Outro Transition Settings")]
    public Animator boxAnimator;
    public GameObject closedBoxObject;
    public GameObject openBoxObject;
    public GameObject[] extraObjectsToHide; // NEW: Drag Clouds, Tapes, and Planes here!
    public AudioSource sfxSource;
    public AudioClip catThrowSound;
    public AudioClip hoverSound;
    public Image backgroundImage;
    public Sprite loadingScreenSprite;
    public float timeToWaitForThrow = 1.5f;

    void Start()
    {
        controllers = Gamepad.all.ToArray();
        Debug.Log("Detected controllers: " + controllers.Length);

        assignedControllers = new int[slots.Length];
        isReady = new bool[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            assignedControllers[i] = -1;
            isReady[i] = false;

            slots[i].joinPanel.SetActive(true);
            slots[i].menuPanel.SetActive(false);
            slots[i].readyPanel.SetActive(false);

            if (slots[i].previewCamera != null) slots[i].previewCamera.gameObject.SetActive(false);
            if (slots[i].previewImage != null) slots[i].previewImage.gameObject.SetActive(false);
            slots[i].spawnedModel = null;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.controllerAssignments = new int[slots.Length];
            for (int i = 0; i < slots.Length; i++)
                GameManager.Instance.controllerAssignments[i] = -1;
        }

        // Start the Intro Sequence!
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        canInteract = false; // Controllers OFF

        CanvasGroup[] joinGroups = new CanvasGroup[slots.Length];

        // Give every Join Panel a CanvasGroup and make it invisible (Alpha = 0)
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].joinPanel != null)
            {
                joinGroups[i] = slots[i].joinPanel.GetComponent<CanvasGroup>();
                if (joinGroups[i] == null)
                {
                    joinGroups[i] = slots[i].joinPanel.AddComponent<CanvasGroup>();
                }
                joinGroups[i].alpha = 0f;
            }
        }

        // Wait for 120 frames (2 seconds)
        yield return new WaitForSeconds(introDelay);

        // Fade the "A Join" text in smoothly!
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);

            for (int i = 0; i < joinGroups.Length; i++)
            {
                if (joinGroups[i] != null) joinGroups[i].alpha = currentAlpha;
            }
            yield return null;
        }

        for (int i = 0; i < joinGroups.Length; i++)
        {
            if (joinGroups[i] != null) joinGroups[i].alpha = 1f;
        }

        canInteract = true; // Controllers ON!
    }

    void Update()
    {
        // If the intro isn't finished, ignore ALL controller input
        if (!canInteract) return;

        for (int c = 0; c < controllers.Length; c++)
        {
            Gamepad pad = controllers[c];
            if (pad == null) continue;

            if (pad.buttonSouth.wasPressedThisFrame)
            {
                int playerIndex = GetPlayerIndexFromController(c);
                if (playerIndex != -1)
                {
                    HandleReadyPress(playerIndex);
                    continue;
                }
                TryAssignController(c);
            }

            if (pad.buttonEast.wasPressedThisFrame)
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
                if (assignedControllers[p] != c) continue;

                var ui = slots[p].customizationUI;
                if (ui == null) continue;

                Vector2 stick = pad.leftStick.ReadValue();

                if (stick.y > 0.5f && !slots[p].stickInUse)
                {
                    ui.MoveSelection(-1);
                    slots[p].stickInUse = true;
                }
                else if (stick.y < -0.5f && !slots[p].stickInUse)
                {
                    ui.MoveSelection(1);
                    slots[p].stickInUse = true;
                }

                if (Mathf.Abs(stick.y) < 0.2f) slots[p].stickInUse = false;

                int category = ui.GetCurrentCategoryIndex();
                var customization = slots[p].spawnedModel.GetComponentInChildren<PlayerCustomization>();

                if (pad.rightTrigger.wasPressedThisFrame)
                {
                    if (category == 0) ui.ChangeName(1, p);
                    else if (category == 1) ui.ChangeColor(1, slots[p].spawnedModel, p);
                    else if (category == 2) customization.ChangeHead(1);
                    else if (category == 3) customization.ChangeTorso(1);
                    else if (category == 4) customization.ChangeBottom(1);
                }

                if (pad.leftTrigger.wasPressedThisFrame)
                {
                    if (category == 0) ui.ChangeName(-1, p);
                    else if (category == 1) ui.ChangeColor(-1, slots[p].spawnedModel, p);
                    else if (category == 2) customization.ChangeHead(-1);
                    else if (category == 3) customization.ChangeTorso(-1);
                    else if (category == 4) customization.ChangeBottom(-1);
                }
            }
        }
    }

    void TryAssignController(int controllerIndex)
    {
        for (int i = 0; i < assignedControllers.Length; i++)
        {
            if (assignedControllers[i] == controllerIndex) return;
        }

        for (int player = 0; player < assignedControllers.Length; player++)
        {
            if (assignedControllers[player] == -1)
            {
                assignedControllers[player] = controllerIndex;

                slots[player].joinPanel.SetActive(false);
                slots[player].menuPanel.SetActive(true);
                slots[player].customizationUI.gameObject.SetActive(true);

                if (slots[player].customizationUI != null) slots[player].customizationUI.Initialize();

                slots[player].previewCamera.gameObject.SetActive(true);
                slots[player].previewImage.texture = slots[player].previewCamera.targetTexture;
                slots[player].previewImage.gameObject.SetActive(true);

                slots[player].spawnedModel = Instantiate(
                    playerModelPrefab,
                    slots[player].modelSpawnPoint.position,
                    slots[player].modelSpawnPoint.rotation,
                    slots[player].modelSpawnPoint);

                var customization = slots[player].spawnedModel.GetComponentInChildren<PlayerCustomization>();
                if (customization != null) customization.Initialize();

                int defaultColorIndex = player;
                slots[player].customizationUI.SetColorIndex(defaultColorIndex, slots[player].spawnedModel);

                var movement = slots[player].spawnedModel.GetComponent<Playermovement>();
                if (movement != null)
                {
                    movement.enabled = false;
                    movement.playerID = player;
                }

                if (GameManager.Instance != null)
                    GameManager.Instance.controllerAssignments[player] = controllerIndex;

                return;
            }
        }
    }

    int GetPlayerIndexFromController(int controllerIndex)
    {
        for (int i = 0; i < assignedControllers.Length; i++)
        {
            if (assignedControllers[i] == controllerIndex) return i;
        }
        return -1;
    }

    void HandleReadyPress(int player)
    {
        if (!slots[player].menuPanel.activeSelf) return;

        if (!isReady[player])
        {
            isReady[player] = true;
            slots[player].readyPanel.SetActive(true);
        }
        else
        {
            isReady[player] = false;
            slots[player].readyPanel.SetActive(false);
        }

        CheckStartCondition();
    }

    void CheckStartCondition()
    {
        if (countdownStarted) return;

        int readyCount = 0;
        for (int i = 0; i < isReady.Length; i++)
        {
            if (isReady[i]) readyCount++;
        }

        if (readyCount >= slots.Length)
        {
            StartCoroutine(StartCountdown());
        }
    }

    IEnumerator StartCountdown()
    {
        countdownStarted = true;
        countdownPanel.SetActive(true);

        int timeLeft = 5;

        while (timeLeft > 0)
        {
            if (GetReadyCount() < slots.Length)
            {
                countdownPanel.SetActive(false);
                countdownStarted = false;
                yield break;
            }

            countdownText.text = timeLeft.ToString();
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }

        StartCoroutine(ThrowBoxTransition());
    }

    // --- THE THROW TRANSITION ---
    IEnumerator ThrowBoxTransition()
    {
        canInteract = false;
        countdownPanel.SetActive(false);

        // 1. THE SWAP MAGIC
        if (openBoxObject != null) openBoxObject.SetActive(false);
        if (closedBoxObject != null) closedBoxObject.SetActive(true);

        // --- NEW: HIDE EXTRA JUNK! ---
        // Vaporize the clouds, tapes, and 3D planes instantly!
        if (extraObjectsToHide != null)
        {
            foreach (GameObject obj in extraObjectsToHide)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        // Instantly hide all player UI 
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].joinPanel.SetActive(false);
            slots[i].menuPanel.SetActive(false);
            slots[i].readyPanel.SetActive(false);

            if (slots[i].previewCamera != null) slots[i].previewCamera.gameObject.SetActive(false);
            if (slots[i].previewImage != null) slots[i].previewImage.gameObject.SetActive(false);
        }

        // 2. Play the Sounds
        if (sfxSource != null)
        {
            if (catThrowSound != null) sfxSource.PlayOneShot(catThrowSound);
            if (hoverSound != null) sfxSource.PlayOneShot(hoverSound);
        }

        // 3. Trigger the Box Throw Animation
        if (boxAnimator != null)
        {
            boxAnimator.SetTrigger("ThrowRight");
        }

        // 4. Wait for the box to fly off screen
        yield return new WaitForSeconds(timeToWaitForThrow);

        // 5. Swap the background image
        if (backgroundImage != null && loadingScreenSprite != null)
        {
            backgroundImage.sprite = loadingScreenSprite;
        }

        yield return new WaitForSeconds(0.5f);

        // 6. Launch the game!
        StartGame();
    }

    int GetReadyCount()
    {
        int count = 0;
        for (int i = 0; i < isReady.Length; i++)
            if (isReady[i]) count++;
        return count;
    }

    void StartGame()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.playerCount = slots.Length;
        GameManager.Instance.controllerAssignments = new int[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            GameManager.Instance.controllerAssignments[i] = assignedControllers[i];
        }

        GameManager.Instance.playerCustomizations.Clear();

        for (int i = 0; i < slots.Length; i++)
        {
            PlayerCustomizationData data = new PlayerCustomizationData();

            if (slots[i].spawnedModel != null)
            {
                var customization = slots[i].spawnedModel.GetComponentInChildren<PlayerCustomization>();
                if (customization != null)
                {
                    data.headIndex = customization.GetHeadIndex();
                    data.torsoIndex = customization.GetTorsoIndex();
                    data.bottomIndex = customization.GetBottomIndex();
                }
            }

            var ui = slots[i].customizationUI;
            if (ui != null)
            {
                data.colorIndex = ui.GetCurrentColorIndex();
                Debug.Log($"Saving Player {i} Color: {data.colorIndex}");
                data.nameIndex = ui.GetCurrentNameIndex();
            }

            GameManager.Instance.playerCustomizations.Add(data);
        }

        SceneManager.LoadScene(RoundManager.Instance.currRound.sceneName);
    }

    void HandleBackPress(int player)
    {
        if (isReady[player])
        {
            isReady[player] = false;
            slots[player].readyPanel.SetActive(false);
            return;
        }

        int controllerIndex = assignedControllers[player];
        assignedControllers[player] = -1;

        slots[player].joinPanel.SetActive(true);

        CanvasGroup cg = slots[player].joinPanel.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;

        slots[player].menuPanel.SetActive(false);
        slots[player].readyPanel.SetActive(false);

        slots[player].previewCamera.gameObject.SetActive(false);
        slots[player].previewImage.gameObject.SetActive(false);

        if (slots[player].spawnedModel != null)
            Destroy(slots[player].spawnedModel);

        slots[player].spawnedModel = null;

        if (GameManager.Instance != null)
            GameManager.Instance.controllerAssignments[player] = -1;
    }

    public bool IsColorTaken(int colorIndex, int requestingPlayer)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i == requestingPlayer) continue;
            if (assignedControllers[i] == -1) continue;

            var ui = slots[i].customizationUI;
            if (ui == null) continue;

            if (ui.GetCurrentColorIndex() == colorIndex) return true;
        }
        return false;
    }

    public bool IsNameTaken(int nameIndex, int requestingPlayer)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i == requestingPlayer) continue;
            if (assignedControllers[i] == -1) continue;

            var ui = slots[i].customizationUI;
            if (ui == null) continue;

            if (ui.GetCurrentNameIndex() == nameIndex) return true;
        }
        return false;
    }
}