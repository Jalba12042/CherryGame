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
        // Force normal time speed
        Time.timeScale = 1f;

        controllers = Gamepad.all.ToArray();
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

        canInteract = true;
    }

    void Update()
    {
        controllers = Gamepad.all.ToArray();

        if (!canInteract) return;

        for (int c = 0; c < controllers.Length; c++)
        {
            Gamepad pad = controllers[c];
            if (pad == null) continue;

            // JOIN / READY (A Button)
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

            // BACK (B Button)
            if (pad.buttonEast.wasPressedThisFrame)
            {
                int playerIndex = GetPlayerIndexFromController(c);
                if (playerIndex != -1)
                {
                    HandleBackPress(playerIndex);
                }
                else
                {
                    if (canInteract && GetAssignedPlayerCount() == 0)
                    {
                        StartCoroutine(BackToMenuTransition());
                    }
                }
            }

            // CUSTOMIZATION NAVIGATION (Stick/Triggers)
            HandleCustomizationInput(pad, c);
        }
    }

    private void HandleCustomizationInput(Gamepad pad, int controllerIndex)
    {
        for (int p = 0; p < slots.Length; p++)
        {
            if (assignedControllers[p] != controllerIndex) continue;
            var ui = slots[p].customizationUI;
            if (ui == null || slots[p].spawnedModel == null) continue;

            Vector2 stick = pad.leftStick.ReadValue();
            if (stick.y > 0.5f && !slots[p].stickInUse) { ui.MoveSelection(-1); slots[p].stickInUse = true; }
            else if (stick.y < -0.5f && !slots[p].stickInUse) { ui.MoveSelection(1); slots[p].stickInUse = true; }
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

    IEnumerator BackToMenuTransition()
    {
        canInteract = false;
        HideAllPlayerUI();

        if (objectsToAnimateOnBack != null)
        {
            foreach (Animator anim in objectsToAnimateOnBack)
            {
                if (anim != null) anim.SetTrigger("Outro");
            }
        }

        yield return new WaitForSeconds(timeToWaitForBackAnim);
        SceneManager.LoadScene(backSceneName);
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

    IEnumerator ThrowBoxTransition()
    {
        canInteract = false;
        countdownPanel.SetActive(false);

        if (openBoxObject != null) openBoxObject.SetActive(false);
        if (closedBoxObject != null) closedBoxObject.SetActive(true);

        if (extraObjectsToHide != null)
        {
            foreach (GameObject obj in extraObjectsToHide) if (obj != null) obj.SetActive(false);
        }

        HideAllPlayerUI();

        if (sfxSource != null)
        {
            if (catThrowSound != null) sfxSource.PlayOneShot(catThrowSound);
            if (hoverSound != null) sfxSource.PlayOneShot(hoverSound);
        }

        if (boxAnimator != null) boxAnimator.SetTrigger("ThrowRight");

        yield return new WaitForSeconds(timeToWaitForThrow);

        if (backgroundImage != null && loadingScreenSprite != null) backgroundImage.sprite = loadingScreenSprite;

        yield return new WaitForSeconds(0.5f);
        StartGame();
    }

    void TryAssignController(int controllerIndex)
    {
        for (int i = 0; i < assignedControllers.Length; i++) if (assignedControllers[i] == controllerIndex) return;

        for (int player = 0; player < assignedControllers.Length; player++)
        {
            if (assignedControllers[player] == -1)
            {
                //assignedControllers[player] = controllerIndex;
                assignedControllers[player] = controllers[controllerIndex].deviceId;
                slots[player].joinPanel.SetActive(false);
                slots[player].menuPanel.SetActive(true);
                slots[player].customizationUI.gameObject.SetActive(true);

                if (slots[player].customizationUI != null) slots[player].customizationUI.Initialize();

                slots[player].previewCamera.gameObject.SetActive(true);
                slots[player].previewImage.texture = slots[player].previewCamera.targetTexture;
                slots[player].previewImage.gameObject.SetActive(true);

                slots[player].spawnedModel = Instantiate(playerModelPrefab, slots[player].modelSpawnPoint.position, slots[player].modelSpawnPoint.rotation, slots[player].modelSpawnPoint);

                var customization = slots[player].spawnedModel.GetComponentInChildren<PlayerCustomization>();
                if (customization != null) customization.Initialize();

                slots[player].customizationUI.SetColorIndex(player, slots[player].spawnedModel);

                //if (GameManager.Instance != null) GameManager.Instance.controllerAssignments[player] = controllerIndex;
                if (GameManager.Instance != null)
                    GameManager.Instance.controllerAssignments[player] = controllers[controllerIndex].deviceId;
                return;
            }
        }
    }

    void HandleReadyPress(int player)
    {
        if (!slots[player].menuPanel.activeSelf) return;
        isReady[player] = !isReady[player];
        slots[player].readyPanel.SetActive(isReady[player]);
        CheckStartCondition();
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

    void HandleBackPress(int player)
    {
        if (isReady[player]) { isReady[player] = false; slots[player].readyPanel.SetActive(false); return; }

        assignedControllers[player] = -1;
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

    int GetAssignedPlayerCount() { int c = 0; foreach (int a in assignedControllers) if (a != -1) c++; return c; }
    int GetReadyCount() { int c = 0; foreach (bool r in isReady) if (r) c++; return c; }
    //int GetPlayerIndexFromController(int cIdx) { for (int i = 0; i < assignedControllers.Length; i++) if (assignedControllers[i] == cIdx) return i; return -1; }

    int GetPlayerIndexFromController(int controllerIndex)
    {
        Gamepad pad = controllers[controllerIndex];

        if (pad == null)
            return -1;

        int deviceId = pad.deviceId;

        for (int i = 0; i < assignedControllers.Length; i++)
        {
            if (assignedControllers[i] == deviceId)
                return i;
        }

        return -1;
    }

    // --- THE MISSING METHODS ARE RIGHT HERE ---
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