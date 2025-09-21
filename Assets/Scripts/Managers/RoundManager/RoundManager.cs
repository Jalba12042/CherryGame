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

    private float currRoundDurationInSecs;

    private bool roundSelected;

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
                StartCoroutine(StartRound());
            }
            // then every frame we check if the round is over
            if (currRoundProgress >= currRoundDurationInSecs)
            {
                StopCoroutine(StartRound());
                roundSelected = false;
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
        loadRoundData();
    }

    // loads in info based on current round
    private void loadRoundData()
    {
        currRoundDurationInSecs = roundList[currRoundIndex].roundTimeInSeconds;
    }

    // our game timer
    private IEnumerator StartRound()
    {
        while (currRoundProgress < currRoundDurationInSecs)
        {
            currRoundProgress += Time.deltaTime;
            yield return null;
        }

        currRoundProgress = currRoundDurationInSecs;
    }
}
