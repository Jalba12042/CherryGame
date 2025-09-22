using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Stores how many players were chosen in the menu
    public int playerCount;
    public int[] controllerAssignments;

    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject button;

    private void Update()
    {
        // just for our testing: we have a timer and a restart button that changes depending on gamestate
        if (currGameState == GameState.Round)
        {
            timerText.text = "Timer: " + RoundManager.Instance.currRoundProgress;
            button.SetActive(false);
        }
        else
        {
            timerText.text = "";
            button.SetActive(true);
        }
    }
    public enum GameState
    {
        Shop,
        Round
    }
    public GameState currGameState;
    private void Awake()
    {
        // If no instance exists yet, make this the instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object when switching between scenes
        }
        else
        {
            Destroy(gameObject); // If another GameManager already exists, destroy this one
        }
    }

    // temp button method to test round starting
    public void StartRoundButton()
    {
        currGameState = GameState.Round;
    }
}