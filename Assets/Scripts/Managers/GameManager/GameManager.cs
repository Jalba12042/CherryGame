using UnityEngine;
using TMPro;
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

    [Header("Starting Scene")]
    public string firstScene;


    public enum GameState
    {
        Shop,
        Round
    }
    public GameState currGameState;

    private void Update()
    {
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
        else
        {
            timerText = GameObject.FindWithTag("Timer").GetComponent<TMP_Text>();
            button = GameObject.FindWithTag("Button");
        }
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
}