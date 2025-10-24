using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Stores how many players were chosen in the menu
    public int playerCount;
    public int[] controllerAssignments;

    // Stores the total score between rounds for each player
    public int[] playerTotalScores;

    [SerializeField] private TMP_Text timerText;

    // Controller navigation
    private Button[] menuButtons;
    private int currentIndex = 0;
    private bool canMove = true;
    private float deadzone = 0.5f;

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

            playerTotalScores = new int[playerCount];
            for (int i = 0; i < playerCount; i++)
                playerTotalScores[i] = 0;

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
        if (currGameState != GameState.Round && menuButtons != null && menuButtons.Length > 0 && Gamepad.all.Count > 0)
        {
            var gamepad = Gamepad.all[0];
            Vector2 move = gamepad.leftStick.ReadValue();

            // Navigation
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

            // Confirm selection
            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                menuButtons[currentIndex].onClick.Invoke();
            }
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
