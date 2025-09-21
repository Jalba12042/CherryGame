using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int playerCount;
    public int[] controllerAssignments;

    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject button;

    private void Update()
    {
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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // temp button method to test round starting
    public void StartRoundButton()
    {
        currGameState = GameState.Round;
    }
}