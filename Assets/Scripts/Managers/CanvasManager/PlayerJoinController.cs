using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class PlayerJoinController : MonoBehaviour
{
    public PlayerSlots[] slots;
    public GameObject playerModelPrefab;

    private bool[] isReady;
    private int[] assignedControllers;

    public GameObject countdownPanel;
    public TextMeshProUGUI countdownText;
    private bool countdownStarted = false;

    [Header("Intro Animation Settings")]
    public float introDelay = 2.0f;
    public float fadeDuration = 0.5f;
    private bool canInteract = false;

    [Header("Leave Lobby Prompt")]
    public GameObject leaveLobbyPromptPanel;
    public GameObject yesButton;
    public GameObject noButton;
    private bool isLeavePromptActive = false;

    [Header("Dynamic Layout Expansion (Sequential)")]
    public float promptFadeDuration = 1.6f;
    public GameObject p3StartPrompt;
    public Animator[] p3Animators;
    public GameObject p4StartPrompt;
    public Animator[] p4Animators;
    public KeyCode addPlayersKey = KeyCode.Equals;
    public int currentAllowedPlayers = 2;
    private bool isExpanding = false;

    [Header("Outro Transition Settings (Ready Up)")]
    public Animator boxAnimator;
    public GameObject closedBoxObject;
    public GameObject openBoxObject;
    public GameObject[] extraObjectsToHide;
    public AudioSource sfxSource;
    public AudioClip catThrowSound;
    public AudioClip hoverSound;
    public AudioClip slideSound;
    public Image backgroundImage;
    public Sprite loadingScreenSprite;
    public float timeToWaitForThrow = 1.5f;

    [Header("Curtain Transition Settings (Start Game)")]
    public Animator curtainAnimator;
    public string curtainTriggerName = "Close";
    public float curtainCloseDuration = 1.0f;

    [Header("Back Navigation Settings (Press B)")]
    public string backSceneName = "MainMenu";
    public float timeToWaitForBackAnim = 1.0f;
    public Animator[] objectsToAnimateOnBack;

    [Header("Paper Transition Settings")]
    public Animator paperTransitionAnimator;
    public GameObject fakeMainMenuBackground;
    [Tooltip("Type the EXACT name of your reverse parameter from the Animator here!")]
    public string paperReverseTriggerName = "PaperReverseTrigger"; // <-- NEW VARIABLE!
    public float paperAnimationDuration = 1.0f;

    void Start()
    {
        Time.timeScale = 1f;

        assignedControllers = new int[slots.Length];
        isReady = new bool[slots.Length];
        currentAllowedPlayers = 2;
        isExpanding = false;
        isLeavePromptActive = false;

        if (leaveLobbyPromptPanel != null) leaveLobbyPromptPanel.SetActive(false);

        for (int i = 0; i < slots.Length; i++)
        {
            assignedControllers[i] = -1;
            isReady[i] = false;

            slots[i].joinPanel.SetActive(i < currentAllowedPlayers);
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

        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        canInteract = false;
        CanvasGroup[] joinGroups = new CanvasGroup[slots.Length];

        for (int i = 0; i < currentAllowedPlayers; i++)
        {
            if (slots[i].joinPanel != null)
            {
                joinGroups[i] = slots[i].joinPanel.GetComponent<CanvasGroup>();
                if (joinGroups[i] == null) joinGroups[i] = slots[i].joinPanel.AddComponent<CanvasGroup>();
                joinGroups[i].alpha = 0f;
            }
        }

        CanvasGroup p3PromptGroup = null;
        CanvasGroup p4PromptGroup = null;

        if (p3StartPrompt != null)
        {
            p3StartPrompt.SetActive(true);
            p3PromptGroup = p3StartPrompt.GetComponent<CanvasGroup>();
            if (p3PromptGroup == null) p3PromptGroup = p3StartPrompt.AddComponent<CanvasGroup>();
            p3PromptGroup.alpha = 0f;
        }

        if (p4StartPrompt != null)
        {
            p4StartPrompt.SetActive(true);
            p4PromptGroup = p4StartPrompt.GetComponent<CanvasGroup>();
            if (p4PromptGroup == null) p4PromptGroup = p4StartPrompt.AddComponent<CanvasGroup>();
            p4PromptGroup.alpha = 0f;
        }

        yield return new WaitForSeconds(introDelay);
        StartCoroutine(FadeInPrompts(p3PromptGroup, p4PromptGroup));

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            for (int i = 0; i < currentAllowedPlayers; i++)
                if (joinGroups[i] != null) joinGroups[i].alpha = alpha;
            yield return null;
        }

        for (int i = 0; i < currentAllowedPlayers; i++)
            if (joinGroups[i] != null) joinGroups[i].alpha = 1f;

        canInteract = true;
    }

    private IEnumerator FadeInPrompts(CanvasGroup p3Group, CanvasGroup p4Group)
    {
        float elapsed = 0f;
        while (elapsed < promptFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / promptFadeDuration);
            if (p3Group != null) p3Group.alpha = alpha;
            if (p4Group != null) p4Group.alpha = alpha;
            yield return null;
        }
        if (p3Group != null) p3Group.alpha = 1f;
        if (p4Group != null) p4Group.alpha = 1f;
    }

    void Update()
    {
        if (!canInteract) return;

        // --- Leave Lobby Prompt Intercept ---
        if (isLeavePromptActive)
        {
            if (InputManager.Instance.GetMenuBackDown())
            {
                CancelLeaveLobby();
            }
            else if (InputManager.Instance.GetMenuConfirmDown())
            {
                ConfirmLeaveLobby();
            }
            return;
        }

        bool playerBackedOutThisFrame = false;

        // --- DYNAMIC EXPANSION TRIGGER ---
        if (currentAllowedPlayers < slots.Length)
        {
            bool triggerExpansion = false;

            if (Input.GetKeyDown(addPlayersKey)) triggerExpansion = true;

            if (InputManager.Instance.GetUnassignedKeyboardJoin())
            {
                int assignedCount = 0;
                for (int i = 0; i < currentAllowedPlayers; i++)
                {
                    if (assignedControllers[i] != -1) assignedCount++;
                }
                if (assignedCount == currentAllowedPlayers) triggerExpansion = true;
            }

            for (int c = 0; c < Gamepad.all.Count; c++)
            {
                Gamepad pad = Gamepad.all[c];
                if (pad != null && pad.startButton.wasPressedThisFrame)
                {
                    triggerExpansion = true;
                    break;
                }
            }

            if (triggerExpansion && !isExpanding) StartCoroutine(ExpandLayoutSequence());
        }

        if (InputManager.CurrentMode == InputManager.InputMode.Arcade)
        {
            for (int p = 0; p < currentAllowedPlayers; p++)
            {
                int playerID = p + 1;
                if (InputManager.Instance.GetConfirmDown(playerID))
                {
                    if (assignedControllers[p] != -1) HandleReadyPress(p);
                    else TryAssignArcadePlayer(p);
                }
                if (InputManager.Instance.GetBackDown(playerID))
                {
                    if (assignedControllers[p] != -1)
                    {
                        HandleBackPress(p);
                        playerBackedOutThisFrame = true;
                    }
                    else if (GetAssignedPlayerCount() == 0 && !playerBackedOutThisFrame) ShowLeavePrompt();
                }
                if (assignedControllers[p] != -1) HandleCustomizationInputArcade(p);
            }
        }
        else
        {
            for (int p = 0; p < currentAllowedPlayers; p++)
            {
                if (assignedControllers[p] == -1) continue;

                int playerID = p + 1;

                if (InputManager.Instance.GetConfirmDown(playerID)) HandleReadyPress(p);
                if (InputManager.Instance.GetBackDown(playerID))
                {
                    HandleBackPress(p);
                    playerBackedOutThisFrame = true;
                }

                if (InputManager.Instance.IsKeyboardPlayer(playerID))
                {
                    HandleCustomizationInputKeyboard(p);
                }
                else
                {
                    HandleCustomizationInputGamepad(p);
                }
            }
        }

        if (InputManager.Instance.GetUnassignedKeyboardJoin())
        {
            TryAssignKeyboardPlayer();
        }
        else if (InputManager.Instance.GetUnassignedKeyboardBack() && GetAssignedPlayerCount() == 0 && !playerBackedOutThisFrame)
        {
            ShowLeavePrompt();
        }

        for (int c = 0; c < Gamepad.all.Count; c++)
        {
            Gamepad pad = Gamepad.all[c];
            if (pad == null || GetPlayerIndexFromController(c) != -1) continue;

            if (pad.buttonSouth.wasPressedThisFrame) TryAssignController(c);
            if (pad.buttonEast.wasPressedThisFrame && GetAssignedPlayerCount() == 0 && !playerBackedOutThisFrame)
                ShowLeavePrompt();
        }
    }

    void ShowLeavePrompt()
    {
        if (leaveLobbyPromptPanel != null)
            leaveLobbyPromptPanel.SetActive(true);

        isLeavePromptActive = true;

        if (noButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(noButton);
        }
    }

    public void ConfirmLeaveLobby()
    {
        isLeavePromptActive = false;
        if (leaveLobbyPromptPanel != null) leaveLobbyPromptPanel.SetActive(false);
        StartCoroutine(BackToMenuTransition());
    }

    public void CancelLeaveLobby()
    {
        isLeavePromptActive = false;
        if (leaveLobbyPromptPanel != null) leaveLobbyPromptPanel.SetActive(false);
    }

    private IEnumerator ExpandLayoutSequence()
    {
        isExpanding = true;
        int newPlayerIndex = currentAllowedPlayers;

        if (sfxSource != null && slideSound != null)
            sfxSource.PlayOneShot(slideSound);

        Animator[] animsToPlay = null;

        if (newPlayerIndex == 2)
        {
            animsToPlay = p3Animators;
            if (p3StartPrompt != null) p3StartPrompt.SetActive(false);
        }
        else if (newPlayerIndex == 3)
        {
            animsToPlay = p4Animators;
            if (p4StartPrompt != null) p4StartPrompt.SetActive(false);
        }

        if (animsToPlay != null)
            foreach (Animator anim in animsToPlay)
                if (anim != null) anim.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.9f);

        currentAllowedPlayers++;

        slots[newPlayerIndex].joinPanel.SetActive(true);
        CanvasGroup cg = slots[newPlayerIndex].joinPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = slots[newPlayerIndex].joinPanel.AddComponent<CanvasGroup>();

        StartCoroutine(FadeInGroup(cg, fadeDuration));

        isExpanding = false;
    }

    private IEnumerator FadeInGroup(CanvasGroup cg, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    private void HandleCustomizationInputGamepad(int p)
    {
        var ui = slots[p].customizationUI;
        if (ui == null || slots[p].spawnedModel == null) return;

        int playerID = p + 1;
        Vector2 stick = InputManager.Instance.GetMove(playerID);

        if (stick.y > 0.5f && !slots[p].stickInUse) { ui.MoveSelection(-1); slots[p].stickInUse = true; }
        else if (stick.y < -0.5f && !slots[p].stickInUse) { ui.MoveSelection(1); slots[p].stickInUse = true; }
        if (Mathf.Abs(stick.y) < 0.2f) slots[p].stickInUse = false;

        int category = ui.GetCurrentCategoryIndex();
        var customization = slots[p].spawnedModel.GetComponentInChildren<PlayerCustomization>();

        bool rightPressed = stick.x > 0.5f;
        bool leftPressed = stick.x < -0.5f;

        Gamepad pad = InputManager.Instance.GetAssignedGamepad(playerID);
        if (pad != null)
        {
            rightPressed |= pad.dpad.right.wasPressedThisFrame;
            leftPressed |= pad.dpad.left.wasPressedThisFrame;
        }

        if (rightPressed && !slots[p].horizontalStickInUse)
        {
            slots[p].horizontalStickInUse = true;
            if (category == 0) ui.ChangeName(1, p);
            else if (category == 1) ui.ChangeColor(1, slots[p].spawnedModel, p);
            else if (category == 2) customization.ChangeHead(1);
            else if (category == 3) customization.ChangeFace(1);
            else if (category == 4) customization.ChangeTorso(1);
            else if (category == 5) customization.ChangeBottom(1);
        }
        else if (leftPressed && !slots[p].horizontalStickInUse)
        {
            slots[p].horizontalStickInUse = true;
            if (category == 0) ui.ChangeName(-1, p);
            else if (category == 1) ui.ChangeColor(-1, slots[p].spawnedModel, p);
            else if (category == 2) customization.ChangeHead(-1);
            else if (category == 3) customization.ChangeFace(-1);
            else if (category == 4) customization.ChangeTorso(-1);
            else if (category == 5) customization.ChangeBottom(-1);
        }

        if (Mathf.Abs(stick.x) < 0.2f && (pad == null || (!pad.dpad.left.isPressed && !pad.dpad.right.isPressed)))
        {
            slots[p].horizontalStickInUse = false;
        }

        if (InputManager.Instance.GetGrabDown(playerID))
        {
            if (category == 0) ui.ChangeName(1, p);
            else if (category == 1) ui.ChangeColor(1, slots[p].spawnedModel, p);
            else if (category == 2) customization.ChangeHead(1);
            else if (category == 3) customization.ChangeFace(1);
            else if (category == 4) customization.ChangeTorso(1);
            else if (category == 5) customization.ChangeBottom(1);
        }

        if (InputManager.Instance.GetThrowDown(playerID))
        {
            if (category == 0) ui.ChangeName(-1, p);
            else if (category == 1) ui.ChangeColor(-1, slots[p].spawnedModel, p);
            else if (category == 2) customization.ChangeHead(-1);
            else if (category == 3) customization.ChangeFace(-1);
            else if (category == 4) customization.ChangeTorso(-1);
            else if (category == 5) customization.ChangeBottom(-1);
        }
    }

    private void HandleCustomizationInputArcade(int p)
    {
        var ui = slots[p].customizationUI;
        if (ui == null || slots[p].spawnedModel == null) return;

        int playerID = p + 1;
        Vector2 stick = InputManager.Instance.GetMove(playerID);

        if (stick.y > 0.5f && !slots[p].stickInUse) { ui.MoveSelection(-1); slots[p].stickInUse = true; }
        else if (stick.y < -0.5f && !slots[p].stickInUse) { ui.MoveSelection(1); slots[p].stickInUse = true; }
        if (Mathf.Abs(stick.y) < 0.2f) slots[p].stickInUse = false;

        int category = ui.GetCurrentCategoryIndex();
        var customization = slots[p].spawnedModel.GetComponentInChildren<PlayerCustomization>();

        if (InputManager.Instance.GetDashDown(playerID))
        {
            if (category == 0) ui.ChangeName(1, p);
            else if (category == 1) ui.ChangeColor(1, slots[p].spawnedModel, p);
            else if (category == 2) customization.ChangeHead(1);
            else if (category == 3) customization.ChangeFace(1);
            else if (category == 4) customization.ChangeTorso(1);
            else if (category == 5) customization.ChangeBottom(1);
        }

        if (InputManager.Instance.GetGrabDown(playerID))
        {
            if (category == 0) ui.ChangeName(-1, p);
            else if (category == 1) ui.ChangeColor(-1, slots[p].spawnedModel, p);
            else if (category == 2) customization.ChangeHead(-1);
            else if (category == 3) customization.ChangeFace(-1);
            else if (category == 4) customization.ChangeTorso(-1);
            else if (category == 5) customization.ChangeBottom(-1);
        }
    }

    private void HandleCustomizationInputKeyboard(int p)
    {
        var ui = slots[p].customizationUI;
        if (ui == null || slots[p].spawnedModel == null) return;

        int playerID = p + 1;
        Vector2 stick = InputManager.Instance.GetMove(playerID);

        if (stick.y > 0.5f && !slots[p].stickInUse) { ui.MoveSelection(-1); slots[p].stickInUse = true; }
        else if (stick.y < -0.5f && !slots[p].stickInUse) { ui.MoveSelection(1); slots[p].stickInUse = true; }
        if (Mathf.Abs(stick.y) < 0.2f) slots[p].stickInUse = false;

        int category = ui.GetCurrentCategoryIndex();
        var customization = slots[p].spawnedModel.GetComponentInChildren<PlayerCustomization>();

        bool rightPressed = stick.x > 0.5f;
        bool leftPressed = stick.x < -0.5f;

        if (rightPressed && !slots[p].horizontalStickInUse)
        {
            slots[p].horizontalStickInUse = true;
            if (category == 0) ui.ChangeName(1, p);
            else if (category == 1) ui.ChangeColor(1, slots[p].spawnedModel, p);
            else if (category == 2) customization.ChangeHead(1);
            else if (category == 3) customization.ChangeFace(1);
            else if (category == 4) customization.ChangeTorso(1);
            else if (category == 5) customization.ChangeBottom(1);
        }
        else if (leftPressed && !slots[p].horizontalStickInUse)
        {
            slots[p].horizontalStickInUse = true;
            if (category == 0) ui.ChangeName(-1, p);
            else if (category == 1) ui.ChangeColor(-1, slots[p].spawnedModel, p);
            else if (category == 2) customization.ChangeHead(-1);
            else if (category == 3) customization.ChangeFace(-1);
            else if (category == 4) customization.ChangeTorso(-1);
            else if (category == 5) customization.ChangeBottom(-1);
        }

        if (Mathf.Abs(stick.x) < 0.2f)
        {
            slots[p].horizontalStickInUse = false;
        }
    }

    void TryAssignController(int controllerIndex)
    {
        for (int i = 0; i < assignedControllers.Length; i++)
            if (assignedControllers[i] == controllerIndex) return;

        for (int player = 0; player < currentAllowedPlayers; player++)
        {
            if (assignedControllers[player] == -1)
            {
                assignedControllers[player] = controllerIndex;
                if (controllerIndex < Gamepad.all.Count)
                    InputManager.Instance.AssignGamepad(player + 1, Gamepad.all[controllerIndex]);
                if (GameManager.Instance != null)
                    GameManager.Instance.controllerAssignments[player] = controllerIndex < Gamepad.all.Count ? Gamepad.all[controllerIndex].deviceId : -1;
                SetupPlayerSlot(player);
                return;
            }
        }
    }

    void TryAssignArcadePlayer(int player)
    {
        if (player >= currentAllowedPlayers || assignedControllers[player] != -1) return;
        assignedControllers[player] = player;
        SetupPlayerSlot(player);
        if (GameManager.Instance != null)
            GameManager.Instance.controllerAssignments[player] = player;
    }

    void TryAssignKeyboardPlayer()
    {
        for (int player = 0; player < currentAllowedPlayers; player++)
        {
            if (assignedControllers[player] == -1)
            {
                assignedControllers[player] = 99;
                InputManager.Instance.AssignKeyboard(player + 1);
                if (GameManager.Instance != null)
                    GameManager.Instance.controllerAssignments[player] = 99;
                SetupPlayerSlot(player);
                return;
            }
        }
    }

    void SetupPlayerSlot(int player)
    {
        slots[player].joinPanel.SetActive(false);
        slots[player].menuPanel.SetActive(true);
        slots[player].customizationUI.gameObject.SetActive(true);

        if (slots[player].customizationUI != null) slots[player].customizationUI.Initialize(player);

        slots[player].previewCamera.gameObject.SetActive(true);
        slots[player].previewImage.texture = slots[player].previewCamera.targetTexture;
        slots[player].previewImage.gameObject.SetActive(true);

        slots[player].spawnedModel = Instantiate(playerModelPrefab,
            slots[player].modelSpawnPoint.position,
            slots[player].modelSpawnPoint.rotation,
            slots[player].modelSpawnPoint);

        var customization = slots[player].spawnedModel.GetComponentInChildren<PlayerCustomization>();
        if (customization != null) customization.Initialize();

        slots[player].customizationUI.SetColorIndex(player, slots[player].spawnedModel);

        ControllerDeviceSwapper swapper = slots[player].menuPanel.GetComponentInChildren<ControllerDeviceSwapper>(true);
        if (swapper != null)
        {
            InputDevice device = InputManager.Instance.GetAssignedGamepad(player + 1);

            if (device == null && InputManager.Instance.IsKeyboardPlayer(player + 1))
            {
                device = Keyboard.current;
            }

            swapper.LockInDeviceIcons(device);
        }
    }

    void HandleReadyPress(int player)
    {
        if (!slots[player].menuPanel.activeSelf) return;
        isReady[player] = !isReady[player];
        slots[player].readyPanel.SetActive(isReady[player]);
        CheckStartCondition();
    }

    void HandleBackPress(int player)
    {
        if (isReady[player]) { isReady[player] = false; slots[player].readyPanel.SetActive(false); return; }

        assignedControllers[player] = -1;
        InputManager.Instance.UnassignGamepad(player + 1);

        slots[player].joinPanel.SetActive(true);
        var cg = slots[player].joinPanel.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;

        slots[player].menuPanel.SetActive(false);
        slots[player].readyPanel.SetActive(false);
        slots[player].previewCamera.gameObject.SetActive(false);
        slots[player].previewImage.gameObject.SetActive(false);

        if (slots[player].spawnedModel != null) Destroy(slots[player].spawnedModel);
        if (GameManager.Instance != null) GameManager.Instance.controllerAssignments[player] = -1;
    }

    void CheckStartCondition()
    {
        if (countdownStarted) return;

        int readyCount = GetReadyCount();
        int assignedCount = GetAssignedPlayerCount();

        if (assignedCount >= 2 && readyCount == assignedCount)
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
            if (GetReadyCount() < GetAssignedPlayerCount())
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

    IEnumerator BackToMenuTransition()
    {
        canInteract = false;
        HideAllPlayerUI();

        if (objectsToAnimateOnBack != null)
        {
            foreach (Animator anim in objectsToAnimateOnBack)
            {
                if (anim != null && anim.gameObject.activeInHierarchy)
                {
                    anim.SetTrigger("Outro");
                }
            }
        }

        yield return new WaitForSeconds(timeToWaitForBackAnim);

        if (fakeMainMenuBackground != null)
        {
            fakeMainMenuBackground.SetActive(true);
        }

        if (paperTransitionAnimator != null)
        {
            paperTransitionAnimator.gameObject.SetActive(true);
            yield return null; // Wake up frame

            Image paperImg = paperTransitionAnimator.GetComponent<Image>();
            if (paperImg != null)
            {
                Color c = paperImg.color;
                c.a = 1f;
                paperImg.color = c;
            }

            // USE OUR NEW VARIABLE INSTEAD OF GUESSING THE NAME!
            if (!string.IsNullOrEmpty(paperReverseTriggerName))
            {
                paperTransitionAnimator.SetTrigger(paperReverseTriggerName);
            }

            yield return new WaitForSeconds(paperAnimationDuration);
        }

        SceneManager.LoadScene(backSceneName);
    }

    IEnumerator ThrowBoxTransition()
    {
        canInteract = false;
        countdownPanel.SetActive(false);

        if (openBoxObject != null) openBoxObject.SetActive(false);
        if (closedBoxObject != null) closedBoxObject.SetActive(true);

        if (extraObjectsToHide != null)
            foreach (GameObject obj in extraObjectsToHide)
                if (obj != null) obj.SetActive(false);

        HideAllPlayerUI();

        if (sfxSource != null)
        {
            if (catThrowSound != null) sfxSource.PlayOneShot(catThrowSound);
            if (hoverSound != null) sfxSource.PlayOneShot(hoverSound);
        }

        if (boxAnimator != null) boxAnimator.SetTrigger("ThrowRight");

        yield return new WaitForSeconds(timeToWaitForThrow);

        if (backgroundImage != null && loadingScreenSprite != null)
            backgroundImage.sprite = loadingScreenSprite;

        if (curtainAnimator != null)
        {
            curtainAnimator.gameObject.SetActive(true);
            curtainAnimator.SetTrigger(curtainTriggerName);
        }

        yield return new WaitForSeconds(curtainCloseDuration);

        StartGame();
    }

    private void HideAllPlayerUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].joinPanel.SetActive(false);
            slots[i].menuPanel.SetActive(false);
            slots[i].readyPanel.SetActive(false);
            if (slots[i].previewCamera != null) slots[i].previewCamera.gameObject.SetActive(false);
            if (slots[i].previewImage != null) slots[i].previewImage.gameObject.SetActive(false);
        }

        if (p3StartPrompt != null) p3StartPrompt.SetActive(false);
        if (p4StartPrompt != null) p4StartPrompt.SetActive(false);
    }

    void StartGame()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.playerCount = GetAssignedPlayerCount();
        GameManager.Instance.playerCustomizations.Clear();

        for (int i = 0; i < currentAllowedPlayers; i++)
        {
            if (assignedControllers[i] == -1) continue;

            PlayerCustomizationData data = new PlayerCustomizationData();
            if (slots[i].spawnedModel != null)
            {
                var cust = slots[i].spawnedModel.GetComponentInChildren<PlayerCustomization>();
                data.headIndex = cust.GetHeadIndex();
                data.faceIndex = cust.GetFaceIndex();
                data.torsoIndex = cust.GetTorsoIndex();
                data.bottomIndex = cust.GetBottomIndex();
            }
            data.colorIndex = slots[i].customizationUI.GetCurrentColorIndex();
            data.nameIndex = slots[i].customizationUI.GetCurrentNameIndex();
            GameManager.Instance.playerCustomizations.Add(data);
        }
        SceneManager.LoadScene(RoundManager.Instance.currRound.sceneName);
    }

    int GetAssignedPlayerCount()
    {
        int count = 0;

        for (int i = 0; i < currentAllowedPlayers; i++)
        {
            if (assignedControllers[i] != -1)
                count++;
        }

        return count;
    }

    int GetReadyCount()
    {
        int count = 0;

        for (int i = 0; i < currentAllowedPlayers; i++)
        {
            if (isReady[i])
                count++;
        }

        return count;
    }

    int GetPlayerIndexFromController(int cIdx) { for (int i = 0; i < assignedControllers.Length; i++) if (assignedControllers[i] == cIdx) return i; return -1; }

    public bool IsColorTaken(int colorIndex, int requestingPlayer)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i == requestingPlayer || assignedControllers[i] == -1) continue;
            var ui = slots[i].customizationUI;
            if (ui != null && ui.GetCurrentColorIndex() == colorIndex) return true;
        }
        return false;
    }

    public bool IsNameTaken(int nameIndex, int requestingPlayer)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i == requestingPlayer || assignedControllers[i] == -1) continue;
            var ui = slots[i].customizationUI;
            if (ui != null && ui.GetCurrentNameIndex() == nameIndex) return true;
        }
        return false;
    }
}