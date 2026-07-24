using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int playerCount;
    public int[] controllerAssignments;
    public int[] playerTotalScores;

    [SerializeField] private TMP_Text timerText;

    private Button[] menuButtons;
    private int currentIndex = 0;
    private bool canMove = true;
    private float deadzone = 0.5f;

    public bool isOnKeyboard = false;

    public List<PlayerCustomizationData> playerCustomizations = new List<PlayerCustomizationData>();

    public enum GameState
    {
        Shop,
        Round,
        Win
    }
    public GameState currGameState;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            playerTotalScores = new int[4];
            controllerAssignments = new int[4];

            for (int i = 0; i < 4; i++)
            {
                playerTotalScores[i] = 0;
                controllerAssignments[i] = -1;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject timerObj = GameObject.FindWithTag("Timer");
        timerText = timerObj != null ? timerObj.GetComponent<TMP_Text>() : null;

        menuButtons = GameObject.FindObjectsByType<Button>(FindObjectsSortMode.None);
        currentIndex = 0;

        if (menuButtons.Length > 0)
            HighlightButton();
    }

    private void Update()
    {
        if (currGameState == GameState.Round && timerText != null && RoundManager.Instance != null)
        {
            timerText.text = "Timer: " + (RoundManager.Instance.currRoundDurationInSecs - (int)RoundManager.Instance.currRoundProgress);
        }

        if (currGameState != GameState.Round && menuButtons != null && menuButtons.Length > 0 && InputManager.Instance != null)
        {
            Vector2 move = InputManager.Instance.GetMove(1);

            if (canMove)
            {
                if (move.y > deadzone)
                {
                    currentIndex = Mathf.Max(0, currentIndex - 1);
                    HighlightButton();
                    canMove = false;
                }
                else if (move.y < -deadzone)
                {
                    currentIndex = Mathf.Min(menuButtons.Length - 1, currentIndex + 1);
                    HighlightButton();
                    canMove = false;
                }
            }

            if (Mathf.Abs(move.y) < 0.2f)
                canMove = true;

            if (InputManager.Instance.GetConfirmDown(1))
                menuButtons[currentIndex].onClick.Invoke();
        }
    }

    private void HighlightButton()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            ColorBlock colors = menuButtons[i].colors;
            colors.normalColor = (i == currentIndex) ? Color.yellow : Color.white;
            menuButtons[i].colors = colors;
        }
    }

    public void StartRoundButton()
    {
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.switchRoundScene();
        }
    }

    public Gamepad GetAssignedGamepad(int playerIndex)
    {
        int assignedDeviceId = controllerAssignments[playerIndex];

        if (assignedDeviceId < 0)
            return null;

        foreach (Gamepad gamepad in Gamepad.all)
        {
            if (gamepad.deviceId == assignedDeviceId)
            {
                return gamepad;
            }
        }

        return null;
    }

    public void RefreshPlayerController(int playerIndex)
    {
        if (RoundManager.Instance == null)
            return;

        if (RoundManager.Instance.playerObjects == null)
            return;

        if (playerIndex < 0 ||
            playerIndex >= RoundManager.Instance.playerObjects.Length)
            return;

        Gamepad gamepad = GetAssignedGamepad(playerIndex);

        if (gamepad == null)
            return;

        GameObject playerObject =
            RoundManager.Instance.playerObjects[playerIndex];

        if (playerObject == null)
            return;

        Playermovement player =
            playerObject.GetComponentInChildren<Playermovement>();

        if (player != null)
        {
            player.assignedGamepad = gamepad;
        }
    }

    /*public Gamepad GetAssignedGamepad(int playerIndex)
    {
        int controllerIndex = controllerAssignments[playerIndex];
        if (controllerIndex >= 0 && controllerIndex < Gamepad.all.Count)
            return Gamepad.all[controllerIndex];
        return null;
    }*/

    // ==========================================
    // --- THE TRUE REBOOT SYSTEM (SOFT RESET) ---
    // ==========================================
    public void HardResetForMainMenu()
    {
        // 1. Wipe player data cleanly
        playerCount = 0;
        playerCustomizations.Clear();

        if (controllerAssignments != null)
        {
            for (int i = 0; i < controllerAssignments.Length; i++) controllerAssignments[i] = -1;
        }

        if (playerTotalScores != null)
        {
            for (int i = 0; i < playerTotalScores.Length; i++) playerTotalScores[i] = 0;
        }

        // 2. SOFT RESET the Round Manager
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.StopAllRoundLogic();
            RoundManager.Instance.currRoundActive = false;
            RoundManager.Instance.currRound = null;
            RoundManager.Instance.currRoundProgress = 0f;
        }

        // 3. --- THE FIX: SOFT RESET the EventManager instead of killing it! ---
        if (EventManager.Instance != null)
        {
            EventManager.Instance.SoftReset();
        }

        // 4. Set state to Shop (Idle)
        currGameState = GameState.Shop;
    }
}