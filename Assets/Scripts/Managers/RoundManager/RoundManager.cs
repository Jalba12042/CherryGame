using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;

    [Header("Current Round Information")]
    public float currRoundProgress;
    public float currRoundDurationInSecs;
    public float currRoundProgressNormalized; // used in events
    public Round currRound;
    public bool currRoundActive;
    public int[] currRoundScores;
    public List<Round> roundList; // list of rounds we can cycle through

    [Header("UI")]
    private TextMeshProUGUI timerText;

    // NEW: A slot to hold your Cardboard Timer Background!
    public GameObject timerBackgroundUI;

    [Header("PowerUp List")]
    public List<GameObject> powerUpsInRotation; // List of all powerups in rotation

    [Header("Flag to allow repeated rounds if we so choose")]
    [SerializeField] private bool allowRepeats; // flag to allow repeated rounds if we so choose

    [Header("Scene Names")]
    [SerializeField] private string shopSceneName;
    [SerializeField] private string controllerSceneName;
    [SerializeField] private string winSceneName;
    [SerializeField] private string gameWinSceneName;

    [SerializeField] private int startTimerInSeconds;
    [SerializeField] private GameObject playerPrefab;

    [Header("Max Score to Win Game")]
    [SerializeField] private int maxScore = 5;

    public GameObject[] playerObjects;
    public List<GameObject> powerupsInPlay;
    public PlayerSpawn currPlayerSpawn;
    private int currRoundIndex;

    public RenderTexture[] playerFaceRenderTextures;

    public int[] roundsWon = { 0, 0, 0, 0 };


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            currRoundProgress = 0;
            currRoundActive = false;
            currRound = null;
            currPlayerSpawn = null;
        }
        else
        {
            Destroy(gameObject);
        }

        GameObject timerGO = GameObject.FindWithTag("Timer");
        if (timerGO != null)
        {
            timerText = timerGO.GetComponent<TextMeshProUGUI>();
            if (timerText == null)
                Debug.LogWarning("Timer GameObject found but no TextMeshProUGUI component attached.");
        }
        else
        {
            Debug.LogWarning("No GameObject with tag 'Timer' found in the scene.");
        }

        // NEW: Ensure the background starts turned OFF
        if (timerBackgroundUI != null)
        {
            timerBackgroundUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (currRound == null && (SceneManager.GetActiveScene().name.Equals(controllerSceneName) || SceneManager.GetActiveScene().name.Equals(shopSceneName) || SceneManager.GetActiveScene().name.Equals("Local Screen"))) // the local screen check will be removed later
        {
            SelectRound();
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

    public void switchRoundScene()
    {
        if (!SceneManager.GetActiveScene().name.Equals(currRound.sceneName))
        {
            SceneManager.LoadSceneAsync(currRound.sceneName);
        }
    }

    // returns the winner index
    private List<int> checkWinIndexes()
    {
        int currWinnerScore = currRoundScores[0];
        List<int> currWinnerIndexes = new List<int>();
        for (int i = 0; i < currRoundScores.Length; i++)
        {
            if (currRoundScores[i] > currWinnerScore)
            {
                currWinnerIndexes.Clear();
                currWinnerIndexes.Add(i);
            }
            else if (currRoundScores[i] == currWinnerScore)
            {
                currWinnerIndexes.Add(i);
            }
        }

        for (int i = 0; i < currWinnerIndexes.Count; i++)
        {
            roundsWon[currWinnerIndexes[i]]++;
        }
        return currWinnerIndexes;
    }

    private List<int> checkGameWinIndexes()
    {
        List<int> currWinnerIndexes = new List<int>();
        for (int i = 0; i < roundsWon.Length; i++)
        {
            if (roundsWon[i] == maxScore)
            {
                currWinnerIndexes.Add(i);
                powerUpsInRotation.Clear();
            }
        }

        return currWinnerIndexes;
    }

    public void BeginRound()
    {
        if (currRound == null || currRoundActive) return;

        currRoundProgress = 0;
        currRoundProgressNormalized = 0;

        currRound.goalObjects = new List<GameObject>();
        EventManager.Instance.eventTextObj = GameObject.FindWithTag("EventText");

        SpawnPlayers();

        currRound.setValues();
        StartCoroutine(StartTimer());
    }


    private void SpawnPlayers()
    {
        currPlayerSpawn = FindFirstObjectByType<PlayerSpawn>();
        playerObjects = new GameObject[GameManager.Instance.playerCount];
        if (!GameManager.Instance.isOnKeyboard)
        {
            for (int i = 0; i < GameManager.Instance.playerCount; i++)
            {
                int assignedControllerIndex = GameManager.Instance.controllerAssignments[i];

                GameObject playerObj = Instantiate(playerPrefab, currPlayerSpawn.spawnPoints[i].position, Quaternion.identity);
                Playermovement player = playerObj.GetComponentInChildren<Playermovement>();
                player.playerIndex = i;
                player.GetComponent<PlayerEscapeUI>().playerIndex = i;

                if (assignedControllerIndex >= 0 && assignedControllerIndex < UnityEngine.InputSystem.Gamepad.all.Count)
                    player.assignedGamepad = UnityEngine.InputSystem.Gamepad.all[assignedControllerIndex];

                PlayerColorAssigner colorAssigner = playerObj.GetComponentInChildren<PlayerColorAssigner>();
                if (colorAssigner != null) colorAssigner.AssignColor(i);

                Camera faceCam = player.GetComponentInChildren<Camera>();
                if (faceCam != null && i < playerFaceRenderTextures.Length)
                    faceCam.targetTexture = playerFaceRenderTextures[i];

                playerObjects[i] = playerObj;
            }
        }
        else
        {
            GameObject playerObj = Instantiate(playerPrefab, currPlayerSpawn.spawnPoints[0].position, Quaternion.identity);
            Playermovement player = playerObj.GetComponentInChildren<Playermovement>();
            player.playerIndex = 0;
            player.GetComponent<PlayerEscapeUI>().playerIndex = 0;

            PlayerColorAssigner colorAssigner = playerObj.GetComponentInChildren<PlayerColorAssigner>();
            if (colorAssigner != null) colorAssigner.AssignColor(0);

            Camera faceCam = player.GetComponentInChildren<Camera>();
            if (faceCam != null && 0 < playerFaceRenderTextures.Length)
                faceCam.targetTexture = playerFaceRenderTextures[0];

            playerObjects[0] = playerObj;
        }
    }

    public IEnumerator StartTimer()
    {
        // NEW: Turn on the background timer UI exactly when the sequence starts
        if (timerBackgroundUI != null)
        {
            timerBackgroundUI.SetActive(true);

            // If you still use the Animator for the flipbook, this will safely trigger it!
            Animator bgAnim = timerBackgroundUI.GetComponent<Animator>();
            if (bgAnim != null) bgAnim.SetTrigger("StartTimer");
        }

        timerText.text = "";
        SetPlayersCanMove(false);

        float timer = 0;
        float maxTimer = 3;
        TMP_Text startTimerText = currRound.startTimerUI.GetComponent<TMP_Text>();
        if (currRound.startTimerUI != null)
        {
            currRound.startTimerUI.SetActive(true);
            while (timer < 3)
            {
                timer += Time.deltaTime;
                startTimerText.text = $"{((int)(maxTimer - timer)) + 1}";
                yield return null;
            }
            currRound.startTimerUI.SetActive(false);
        }

        SetPlayersCanMove(true);

        currRoundActive = true;
        StartCoroutine(RoundTimer());
        StartCoroutine(currRound.StartGoal());
        StartCoroutine(EventManager.Instance.EventTimer());
    }

    public IEnumerator RoundTimer()
    {
        while (currRoundProgress < currRoundDurationInSecs)
        {
            currRoundProgress += Time.deltaTime;
            currRoundProgressNormalized = currRoundProgress / currRoundDurationInSecs;

            if (timerText != null)
            {
                float remaining = currRoundDurationInSecs - currRoundProgress;
                timerText.text = Mathf.CeilToInt(remaining).ToString();
            }

            yield return null;
        }

        currRoundProgress = currRoundDurationInSecs;
        currRoundProgressNormalized = 1f;

        currRoundScores = currRound.ScoreCount();
        List<int> winnerIndexes = checkWinIndexes();
        WinScript.winningPlayers = winnerIndexes;

        List<int> gameWinners = checkGameWinIndexes();
        if (gameWinners.Count != 0)
        {
            GameWinScript.winningPlayers = gameWinners;
            roundsWon = new int[] { 0, 0, 0, 0 };
            SceneManager.LoadSceneAsync(gameWinSceneName);
        }
        else
        {
            SceneManager.LoadSceneAsync(winSceneName);
        }

        currRoundActive = false;
        currRound = null;
    }

    public void SetTimer(TextMeshProUGUI timer)
    {
        timerText = timer;
    }

    private void SetPlayersCanMove(bool value)
    {
        foreach (GameObject playerObj in playerObjects)
        {
            if (playerObj == null) continue;

            Playermovement player = playerObj.GetComponentInChildren<Playermovement>();
            if (player != null)
            {
                player.canMove = value;
            }
        }
    }
}