using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;

    public float currRoundProgress;
    public float currRoundDurationInSecs;
    public List<Round> roundList; // list of rounds we can cycle through
    public Round currRound;
    public bool currRoundActive;
    public int[] currRoundScores;

    [Tooltip("Flag to allow repeated rounds if we so choose")]
    [SerializeField] private bool allowRepeats; // flag to allow repeated rounds if we so choose

    [SerializeField] private string shopSceneName;
    [SerializeField] private int startTimerInSeconds;
    [SerializeField] private GameObject playerPrefab;

    private GameObject[] playerObjects;
    private PlayerSpawn currPlayerSpawn;
    private int currRoundIndex;
    private int startTimer;
    private bool roundSelected;
 
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            roundSelected = false;
            currRoundProgress = 0;
            currRoundActive = false;
            currRound = null;
            currPlayerSpawn = null;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        if (currRound == null && SceneManager.GetActiveScene().name.Equals(shopSceneName))
        {
            SelectRound();
        }
        if (currRound != null && SceneManager.GetActiveScene().name.Equals(currRound.sceneName))
        {
            GameManager.Instance.currGameState = GameManager.GameState.Round;
            // we start with selecting a round and starting the timer
            if (!roundSelected)
            {
                currRoundProgress = 0;
                roundSelected = true;
                currRoundActive = true;
                StartCoroutine(StartRound());
            }
            // then every frame we check if the round is over
            if (currRoundProgress >= currRoundDurationInSecs)
            {
                currRoundScores = currRound.ScoreCount();
                StopCoroutine(StartRound());
                Debug.Log($"Winner is player {checkWinIndex() + 1}");
                roundSelected = false;
                currRoundActive = false;
                currRound = null;
                GameManager.Instance.currGameState = GameManager.GameState.Shop;
                SceneManager.LoadSceneAsync(shopSceneName);
            }
        }
    }

    // randomly selects a round depending on how many we have and if we want to allow repeats 
    private void SelectRound()
    {
        int roundIndex = -1;
        if (allowRepeats)
        {
            while (roundIndex == -1)
            {
                roundIndex = Random.Range(0, roundList.Count);
            }
        }
        else
        {
            while (roundIndex == -1 || roundIndex == currRoundIndex)
            {
                roundIndex = Random.Range(0, roundList.Count);
            }
        }
        
        currRoundIndex = roundIndex;
        currRound = roundList[roundIndex];
        loadRoundData();
    }

    // loads in info based on current round
    private void loadRoundData()
    {
        currRoundDurationInSecs = currRound.roundTimeInSeconds;
    }

    // our game timer
    private IEnumerator StartRound()
    {
        currPlayerSpawn = FindFirstObjectByType<PlayerSpawn>();

        // destroy any left over goal objects
        if (currRound.goalObjects.Count != 0 && currRound.goalObjects != null)
        {
            for (int i = 0; i < currRound.goalObjects.Count; i++)
            {
                Destroy(currRound.goalObjects[i]);
            }
            currRound.goalObjects.Clear();
        }

        // spawn players
        playerObjects = new GameObject[GameManager.Instance.playerCount];
        Debug.Log(GameManager.Instance.playerCount);
        for (int i = 0; i < GameManager.Instance.playerCount; i++)
        {
            playerObjects[i] = Instantiate(playerPrefab, currPlayerSpawn.spawnPoints[i].position, Quaternion.identity);
        }

        // initial timer for round start
        startTimer = 0;
        while (startTimer < startTimerInSeconds)
        {
            yield return new WaitForSeconds(1);
            startTimer++;
            Debug.Log(startTimer);
        }

        // start the round
        StartCoroutine(currRound.StartGoal());
        while (currRoundProgress < currRoundDurationInSecs)
        {
            currRoundProgress += Time.deltaTime;
            yield return null;
        }

        currRoundProgress = currRoundDurationInSecs;
    }
    public void switchRoundScene()
    {
        if (!SceneManager.GetActiveScene().name.Equals(currRound.sceneName))
        {
            SceneManager.LoadSceneAsync(currRound.sceneName);
        }
    }
    private int checkWinIndex()
    {
        int currWinnerScore = currRoundScores[0];
        int currWinnerIndex = 0;
        for (int i = 0; i < currRoundScores.Length; i++)
        {
            if (currRoundScores[i] > currWinnerScore)
            {
                currWinnerIndex = i;
            }
        }
        return currWinnerIndex;
    }
}
