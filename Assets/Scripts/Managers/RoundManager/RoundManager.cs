using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;

    public float currRoundProgress;
    public List<Round> roundList; // list of rounds we can cycle through

    [Tooltip("Flag to allow repeated rounds if we so choose")]
    [SerializeField] private bool allowRepeats; // flag to allow repeated rounds if we so choose

    private int currRoundIndex;

    private Round currRound;

    private float currRoundDurationInSecs;

    private bool roundSelected;
    public bool currRoundActive;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        roundSelected = false;
        currRoundProgress = 0;
        currRoundActive = false;
        currRound = null;
    }
    private void Update()
    {
        if (GameManager.Instance.currGameState == GameManager.GameState.Round)
        {
            // we start with selecting a round and starting the timer
            if (!roundSelected)
            {
                currRoundProgress = 0;
                SelectRound();
                roundSelected = true;
                currRoundActive = true;
                StartCoroutine(StartRound());
            }
            // then every frame we check if the round is over
            if (currRoundProgress >= currRoundDurationInSecs)
            {
                StopCoroutine(StartRound());
                roundSelected = false;
                currRoundActive = false;
                currRound = null;
                GameManager.Instance.currGameState = GameManager.GameState.Shop;
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
        // destroy any left over goal objects
        if (currRound.goalObjects.Count != 0 && currRound.goalObjects != null)
        {
            for (int i = 0; i < currRound.goalObjects.Count; i++)
            {
                Destroy(currRound.goalObjects[i]);
            }
            currRound.goalObjects.Clear();
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
}
