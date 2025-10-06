/*using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    [SerializeField] private GameObject button;

    public enum GameState
    {
        Shop,
        Round,
        Win
    }
    public GameState currGameState;

   private void Update()
     {
         // Only try to get scene-specific references if they're null
         if (timerText == null)
         {
             GameObject timerObj = GameObject.FindWithTag("Timer");
             if (timerObj != null) timerText = timerObj.GetComponent<TMP_Text>();
         }

         if (button == null)
         {
             button = GameObject.FindWithTag("Button");
         }

         // just for our testing: we have a timer and a restart button that changes depending on gamestate
         // uncomment the following to get my test shit working
         if ((timerText != null && button != null))
         {
             if (currGameState == GameState.Round)
             {
                 timerText.text = "Timer: " + (RoundManager.Instance.currRoundDurationInSecs - (int)RoundManager.Instance.currRoundProgress);
                 button.SetActive(false);
             }
             else
             {
                 timerText.text = "";
                 button.SetActive(true);
             }
         }
         //else
         //{
         //    timerText = GameObject.FindWithTag("Timer").GetComponent<TMP_Text>();
        //     button = GameObject.FindWithTag("Button");
        // }
     }



    private void Awake()
    {
        // If no instance exists yet, make this the instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object when switching between scenes

            // set initial player scores to 0
            playerTotalScores = new int[playerCount];
            for (int i = 0; i < playerCount; i++)
            {
                playerTotalScores[i] = 0;
            }
        }
        else
        {
            Destroy(gameObject); // If another GameManager already exists, destroy this one
        }
    }

    // temp button method to test round starting
    public void StartRoundButton()
    {
        RoundManager.Instance.switchRoundScene();
    }


}*/

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
