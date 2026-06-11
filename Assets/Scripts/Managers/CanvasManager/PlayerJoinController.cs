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
    private int[] assignedControllers;

    public GameObject countdownPanel;
    public TextMeshProUGUI countdownText;

    private bool countdownStarted = false;

    [Header("Intro Animation Settings")]
    public float introDelay = 2.0f;
    public float fadeDuration = 0.5f;
    private bool canInteract = false;

    [Header("Outro Transition Settings (Ready Up)")]
    public Animator boxAnimator;
    public GameObject closedBoxObject;
    public GameObject openBoxObject;
    public GameObject[] extraObjectsToHide;
    public AudioSource sfxSource;
    public AudioClip catThrowSound;
    public AudioClip hoverSound;
    public Image backgroundImage;
    public Sprite loadingScreenSprite;
    public float timeToWaitForThrow = 1.5f;

    [Header("Back Navigation Settings (Press B)")]
    public string backSceneName = "MainMenu";
    public float timeToWaitForBackAnim = 1.0f;
    public Animator[] objectsToAnimateOnBack;

    void Start()
    {
        Time.timeScale = 1f;

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

        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        canInteract = false;
        CanvasGroup[] joinGroups = new CanvasGroup[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].joinPanel != null)
            {
                joinGroups[i] = slots[i].joinPanel.GetComponent<CanvasGroup>();
                if (joinGroups[i] == null) joinGroups[i] = slots[i].joinPanel.AddComponent<CanvasGroup>();
                joinGroups[i].alpha = 0f;
            }
        }

        yield return new WaitForSeconds(introDelay);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            for (int i = 0; i < joinGroups.Length; i++)
                if (joinGroups[i] != null) joinGroups[i].alpha = alpha;
            yield return null;
        }

        for (int i = 0; i < joinGroups.Length; i++)
            if (joinGroups[i] != null) joinGroups[i].alpha = 1f;

        canInteract = true;
    }

    void Update()
    {
        if (!canInteract) return;

        // Arcade mode — use InputManager prefix-based input per slot
        if (InputManager.CurrentMode == InputManager.InputMode.Arcade)
        {
            for (int p = 0; p < slots.Length; p++)
            {
                int playerID = p + 1;

                if (InputManager.Instance.GetConfirmDown(playerID))
                {
                    if (assignedControllers[p] != -1)
                        HandleReadyPress(p);
                    else
                        TryAssignArcadePlayer(p);
                }

                if (InputManager.Instance.GetBackDown(playerID))
                {
                    if (assignedControllers[p] != -1)
                        HandleBackPress(p);
                    else if (GetAssignedPlayerCount() == 0)
                        StartCoroutine(BackToMenuTransition());
                }

                if (assignedControllers[p] != -1)
                    HandleCustomizationInputArcade(p);
            }
            return;
        }

        // Keyboard mode — only P1 slot
        if (InputManager.CurrentMode == InputManager.InputMode.Keyboard)
        {
            if (InputManager.Instance.GetConfirmDown(1))
            {
                if (assignedControllers[0] != -1)
                    HandleReadyPress(0);
                else
                    TryAssignKeyboardPlayer();
            }

            if (InputManager.Instance.GetBackDown(1))
            {
                if (assignedControllers[0] != -1)
                    HandleBackPress(0);
                else if (GetAssignedPlayerCount() == 0)
                    StartCoroutine(BackToMenuTransition());
            }

            if (assignedControllers[0] != -1)
                HandleCustomizationInputKeyboard();

            return;
        }

        // Gamepad mode — assigned players go through InputManager; unassigned gamepads polled for join
        for (int p = 0; p < slots.Length; p++)
        {
            if (assignedControllers[p] == -1) continue;
            int playerID = p + 1;

            if (InputManager.Instance.GetConfirmDown(playerID)) HandleReadyPress(p);
            if (InputManager.Instance.GetBackDown(playerID)) HandleBackPress(p);
            HandleCustomizationInputGamepad(p);
        }

        for (int c = 0; c < Gamepad.all.Count; c++)
        {
            Gamepad pad = Gamepad.all[c];
            if (pad == null || GetPlayerIndexFromController(c) != -1) continue;

            if (pad.buttonSouth.wasPressedThisFrame) TryAssignController(c);
            if (pad.buttonEast.wasPressedThisFrame && GetAssignedPlayerCount() == 0)
                StartCoroutine(BackToMenuTransition());
        }
    }

    // ── Customization input ───────────────────────────────────────

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

        if (InputManager.Instance.GetGrabDown(playerID))
        {
            if (category == 0) ui.ChangeName(1, p);
            else if (category == 1) ui.ChangeColor(1, slots[p].spawnedModel, p);
            else if (category == 2) customization.ChangeHead(1);
            else if (category == 3) customization.ChangeTorso(1);
            else if (category == 4) customization.ChangeBottom(1);
        }

        if (InputManager.Instance.GetThrowDown(playerID))
        {
            if (category == 0) ui.ChangeName(-1, p);
            else if (category == 1) ui.ChangeColor(-1, slots[p].spawnedModel, p);
            else if (category == 2) customization.ChangeHead(-1);
            else if (category == 3) customization.ChangeTorso(-1);
            else if (category == 4) customization.ChangeBottom(-1);
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
            else if (category == 3) customization.ChangeTorso(1);
            else if (category == 4) customization.ChangeBottom(1);
        }

        if (InputManager.Instance.GetGrabDown(playerID))
        {
            if (category == 0) ui.ChangeName(-1, p);
            else if (category == 1) ui.ChangeColor(-1, slots[p].spawnedModel, p);
            else if (category == 2) customization.ChangeHead(-1);
            else if (category == 3) customization.ChangeTorso(-1);
            else if (category == 4) customization.ChangeBottom(-1);
        }
    }

    private void HandleCustomizationInputKeyboard()
    {
        var ui = slots[0].customizationUI;
        if (ui == null || slots[0].spawnedModel == null) return;

        Vector2 stick = InputManager.Instance.GetMove(1);

        if (stick.y > 0.5f && !slots[0].stickInUse) { ui.MoveSelection(-1); slots[0].stickInUse = true; }
        else if (stick.y < -0.5f && !slots[0].stickInUse) { ui.MoveSelection(1); slots[0].stickInUse = true; }
        if (Mathf.Abs(stick.y) < 0.2f) slots[0].stickInUse = false;

        int category = ui.GetCurrentCategoryIndex();
        var customization = slots[0].spawnedModel.GetComponentInChildren<PlayerCustomization>();

        if (InputManager.Instance.GetDashDown(1))
        {
            if (category == 0) ui.ChangeName(1, 0);
            else if (category == 1) ui.ChangeColor(1, slots[0].spawnedModel, 0);
            else if (category == 2) customization.ChangeHead(1);
            else if (category == 3) customization.ChangeTorso(1);
            else if (category == 4) customization.ChangeBottom(1);
        }

        if (InputManager.Instance.GetGrabDown(1))
        {
            if (category == 0) ui.ChangeName(-1, 0);
            else if (category == 1) ui.ChangeColor(-1, slots[0].spawnedModel, 0);
            else if (category == 2) customization.ChangeHead(-1);
            else if (category == 3) customization.ChangeTorso(-1);
            else if (category == 4) customization.ChangeBottom(-1);
        }
    }

    // ── Assignment ───────────────────────────────────────────────

    void TryAssignController(int controllerIndex)
    {
        for (int i = 0; i < assignedControllers.Length; i++)
            if (assignedControllers[i] == controllerIndex) return;

        for (int player = 0; player < assignedControllers.Length; player++)
        {
            if (assignedControllers[player] == -1)
            {
                assignedControllers[player] = controllerIndex;
                if (controllerIndex < Gamepad.all.Count)
                    InputManager.Instance.AssignGamepad(player + 1, Gamepad.all[controllerIndex]);
                if (GameManager.Instance != null)
                    GameManager.Instance.controllerAssignments[player] = controllerIndex;
                SetupPlayerSlot(player);
                return;
            }
        }
    }

    void TryAssignArcadePlayer(int player)
    {
        if (assignedControllers[player] != -1) return;
        assignedControllers[player] = player;
        SetupPlayerSlot(player);
        if (GameManager.Instance != null)
            GameManager.Instance.controllerAssignments[player] = player;
    }

    void TryAssignKeyboardPlayer()
    {
        if (assignedControllers[0] != -1) return;
        assignedControllers[0] = 0;
        SetupPlayerSlot(0);
        if (GameManager.Instance != null)
            GameManager.Instance.controllerAssignments[0] = 0;
    }

    void SetupPlayerSlot(int player)
    {
        slots[player].joinPanel.SetActive(false);
        slots[player].menuPanel.SetActive(true);
        slots[player].customizationUI.gameObject.SetActive(true);

        if (slots[player].customizationUI != null) slots[player].customizationUI.Initialize();

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
    }

    // ── Ready / Back ─────────────────────────────────────────────

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
        int readyCount = 0;
        for (int i = 0; i < isReady.Length; i++) if (isReady[i]) readyCount++;
        if (readyCount >= slots.Length) StartCoroutine(StartCountdown());
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

    // ── Transitions ──────────────────────────────────────────────

    IEnumerator BackToMenuTransition()
    {
        canInteract = false;
        HideAllPlayerUI();

        if (objectsToAnimateOnBack != null)
            foreach (Animator anim in objectsToAnimateOnBack)
                if (anim != null) anim.SetTrigger("Outro");

        yield return new WaitForSeconds(timeToWaitForBackAnim);
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

        yield return new WaitForSeconds(0.5f);
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
    }

    void StartGame()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.playerCount = slots.Length;
        GameManager.Instance.playerCustomizations.Clear();

        for (int i = 0; i < slots.Length; i++)
        {
            PlayerCustomizationData data = new PlayerCustomizationData();
            if (slots[i].spawnedModel != null)
            {
                var cust = slots[i].spawnedModel.GetComponentInChildren<PlayerCustomization>();
                data.headIndex = cust.GetHeadIndex();
                data.torsoIndex = cust.GetTorsoIndex();
                data.bottomIndex = cust.GetBottomIndex();
            }
            data.colorIndex = slots[i].customizationUI.GetCurrentColorIndex();
            data.nameIndex = slots[i].customizationUI.GetCurrentNameIndex();
            GameManager.Instance.playerCustomizations.Add(data);
        }
        SceneManager.LoadScene(RoundManager.Instance.currRound.sceneName);
    }

    // ── Helpers ──────────────────────────────────────────────────

    int GetAssignedPlayerCount() { int c = 0; foreach (int a in assignedControllers) if (a != -1) c++; return c; }
    int GetReadyCount() { int c = 0; foreach (bool r in isReady) if (r) c++; return c; }
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