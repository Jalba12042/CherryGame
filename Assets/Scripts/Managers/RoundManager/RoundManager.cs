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
            if (!roundSelected)
            {
                currRoundProgress = 0;
                SelectRound();
                roundSelected = true;
                StartCoroutine(StartRound());
            }
            if (currRoundProgress >= currRoundDurationInSecs)
            {
                StopCoroutine(StartRound());
                roundSelected = false;
                GameManager.Instance.currGameState = GameManager.GameState.Shop;
            }
        }
    }

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

    private void loadRoundData()
    {
        currRoundDurationInSecs = roundList[currRoundIndex].roundTimeInSeconds;
    }

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
