using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Stores how many players were chosen in the menu
    public int playerCount;
    public int[] controllerAssignments;

    // --- THE SCOREBOARD MEMORY BANK ---
    // This safely remembers everyone's wins across all scene reloads!
    public int[] playerTotalScores;

    [SerializeField] private TMP_Text timerText;

    // Controller navigation
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
            DontDestroyOnLoad(gameObject); // Persist across scenes

            // --- THE BULLETPROOF FIX ---
            // ALWAYS create 4 slots so the Scoreboard never crashes when testing!
            playerTotalScores = new int[4];
            controllerAssignments = new int[4];

            for (int i = 0; i < 4; i++)
            {
                playerTotalScores[i] = 0;
                controllerAssignments[i] = -1; // means �unassigned�
            }
            // ---------------------------

            SceneManager.sceneLoaded += OnSceneLoaded; // Listen for scene changes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Refresh timer reference
        GameObject timerObj = GameObject.FindWithTag("Timer");
        timerText = timerObj != null ? timerObj.GetComponent<TMP_Text>() : null;

        // Grab all buttons in the new scene
        menuButtons = GameObject.FindObjectsByType<Button>(FindObjectsSortMode.None);
        currentIndex = 0;

        if (menuButtons.Length > 0)
            HighlightButton();
    }

    private void Update()
    {
        // Timer display for rounds
        if (currGameState == GameState.Round && timerText != null && RoundManager.Instance != null)
        {
            timerText.text = "Timer: " + (RoundManager.Instance.currRoundDurationInSecs - (int)RoundManager.Instance.currRoundProgress);
        }

        // Only allow controller navigation outside of rounds
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

    // Example: temporary method to start the round from a button
    public void StartRoundButton()
    {
        RoundManager.Instance.switchRoundScene();
    }
}