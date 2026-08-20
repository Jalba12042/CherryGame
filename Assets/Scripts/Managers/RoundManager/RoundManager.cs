using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;

    [Header("Current Round Information")]
    public float currRoundProgress;
    public float currRoundDurationInSecs;
    public float currRoundProgressNormalized;
    public Round currRound;
    public bool currRoundActive;
    public int[] currRoundScores;
    public List<Round> roundList;

    [Header("UI")]
    private TextMeshProUGUI timerText;
    public GameObject timerBackgroundUI;

    [Header("PowerUp List")]
    public List<GameObject> powerUpsInRotation;

    [Header("Flag to allow repeated rounds if we so choose")]
    [SerializeField] private bool allowRepeats;

    [Header("Scene Names")]
    [SerializeField] private string shopSceneName;
    [SerializeField] private string controllerSceneName;
    [SerializeField] private string winSceneName;
    [SerializeField] private string gameWinSceneName;

    [SerializeField] private GameObject playerPrefab;

    [Header("Max Score to Win Game")]
    [SerializeField] private int maxScore = 3;

    public GameObject[] playerObjects;
    public List<GameObject> powerupsInPlay;
    public PlayerSpawn currPlayerSpawn;
    private int currRoundIndex = -1; // -1 = no round played yet, so nothing gets excluded on the first pick

    public RenderTexture[] playerFaceRenderTextures;

    private BasketContainer basketContainer;
    public SprinklerManager sprinklerManager;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (Instance == null)
        {
            Instance = this;
            currRoundProgress = 0;
            currRoundActive = false;
            currRound = null;
            currPlayerSpawn = null;
            currRoundIndex = -1;
        }
        else
        {
            Destroy(gameObject);
        }

        RefreshUIReferences();
    }

    public void SetTimer(TextMeshProUGUI timer)
    {
        timerText = timer;
    }

    public void StopAllRoundLogic()
    {
        currRoundActive = false;
        StopAllCoroutines();
        if (timerText != null) timerText.text = "";
    }

    private void RefreshUIReferences()
    {
        TimerLocator locator = FindFirstObjectByType<TimerLocator>();
        if (locator != null) timerText = locator.GetComponent<TextMeshProUGUI>();

        TimerUIManager tManager = FindFirstObjectByType<TimerUIManager>();
        if (tManager != null) timerBackgroundUI = tManager.timerBackgroundObject;

        if (timerBackgroundUI != null) timerBackgroundUI.SetActive(false);
    }

    private void Update()
    {
        if (currRound == null && (SceneManager.GetActiveScene().name.Equals(controllerSceneName) || SceneManager.GetActiveScene().name.Equals(shopSceneName) || SceneManager.GetActiveScene().name.Equals("Local Screen")))
        {
            SelectRound();
        }
    }

    private void SelectRound()
    {
        int roundIndex = -1;
        if (allowRepeats)
        {
            while (roundIndex == -1)
                roundIndex = Random.Range(0, roundList.Count);
        }
        else
        {
            while (roundIndex == -1 || roundIndex == currRoundIndex)
                roundIndex = Random.Range(0, roundList.Count);
        }

        currRoundIndex = roundIndex;
        currRound = roundList[roundIndex];
        loadRoundData();
    }

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

    public void BeginRound()
    {
        if (currRound == null && roundList != null && roundList.Count > 0)
        {
            SelectRound();
        }

        if (sprinklerManager != null) sprinklerManager?.StopSprinklers();
        if (currRound == null || currRoundActive) return;

        currRoundProgress = 0;
        currRoundProgressNormalized = 0;

        currRound.goalObjects = new List<GameObject>();
        EventManager.Instance.eventTextObj = GameObject.FindWithTag("EventText");
        powerupsInPlay.Clear();

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
                GameObject playerObj = Instantiate(playerPrefab, currPlayerSpawn.spawnPoints[i].position, Quaternion.identity);
                var customization = playerObj.GetComponentInChildren<PlayerCustomization>();

                if (customization != null && GameManager.Instance.playerCustomizations.Count > i)
                {
                    customization.playerIndex = i;
                    var data = GameManager.Instance.playerCustomizations[i];
                    customization.ApplyFromData(data);
                }

                if (basketContainer != null && i < basketContainer.baskets.Count)
                {
                    GameObject basketObj = basketContainer.baskets[i];
                    BasketColorSync basket = basketObj.GetComponentInChildren<BasketColorSync>();
                    if (basket != null && customization != null) basket.SetColor(customization.CurrentColorIndex);
                }

                Playermovement player = playerObj.GetComponentInChildren<Playermovement>();
                player.playerIndex = i;
                player.playerID = i + 1;
                player.initialSpawnPosition = currPlayerSpawn.spawnPoints[i].position;
                player.GetComponent<PlayerEscapeUI>().playerIndex = i;

                Gamepad assignedGamepad = GameManager.Instance.GetAssignedGamepad(i);
                if (assignedGamepad != null) player.assignedGamepad = assignedGamepad;

                Camera faceCam = player.GetComponentInChildren<Camera>();
                if (faceCam != null && i < playerFaceRenderTextures.Length) faceCam.targetTexture = playerFaceRenderTextures[i];

                playerObjects[i] = playerObj;
            }
        }
        else
        {
            GameObject playerObj = Instantiate(playerPrefab, currPlayerSpawn.spawnPoints[0].position, Quaternion.identity);
            Playermovement player = playerObj.GetComponentInChildren<Playermovement>();
            player.playerIndex = 0;
            player.playerID = 1;
            player.initialSpawnPosition = currPlayerSpawn.spawnPoints[0].position;
            player.GetComponent<PlayerEscapeUI>().playerIndex = 0;

            PlayerCustomization customization = playerObj.GetComponentInChildren<PlayerCustomization>();
            if (customization != null) customization.AssignColor(0);

            Camera faceCam = player.GetComponentInChildren<Camera>();
            if (faceCam != null && 0 < playerFaceRenderTextures.Length) faceCam.targetTexture = playerFaceRenderTextures[0];

            playerObjects[0] = playerObj;
        }
    }

    public IEnumerator StartTimer()
    {
        RefreshUIReferences();

        if (timerBackgroundUI != null)
        {
            timerBackgroundUI.SetActive(true);
            Animator bgAnim = timerBackgroundUI.GetComponent<Animator>();
            if (bgAnim != null) bgAnim.SetTrigger("StartTimer");
        }

        TimerUIManager tManager = FindFirstObjectByType<TimerUIManager>();
        if (tManager != null) tManager.RevealTimer();

        if (timerText != null) timerText.text = "";

        SetPlayersCanMove(false);

        float timer = 0;
        float maxTimer = 3;
        if (currRound != null && currRound.startTimerUI != null)
        {
            TMP_Text startTimerText = currRound.startTimerUI.GetComponent<TMP_Text>();
            currRound.startTimerUI.SetActive(true);
            while (timer < 3)
            {
                timer += Time.deltaTime;
                if (startTimerText != null) startTimerText.text = $"{((int)(maxTimer - timer)) + 1}";
                yield return null;
            }
            currRound.startTimerUI.SetActive(false);
        }

        SetPlayersCanMove(true);
        currRoundActive = true;

        if (sprinklerManager != null) sprinklerManager?.StartSprinklers();

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

            currRound.RoundUpdate();
            yield return null;
        }

        currRoundProgress = currRoundDurationInSecs;
        currRoundProgressNormalized = 1f;
        if (timerText != null) timerText.text = "0";

        SetPlayersCanMove(false);
        if (sprinklerManager != null) sprinklerManager.StopSprinklers();

        // --- NEW SCORING LOGIC FIX ---
        currRoundScores = currRound.ScoreCount();
        List<int> gameWinners = UpdateScoresAndCheckGameWin();

        string sceneToLoad;
        if (gameWinners.Count > 0)
        {
            sceneToLoad = gameWinSceneName; // First to 3 hit! Go to final screen
        }
        else
        {
            sceneToLoad = winSceneName; // Keep playing
        }

        EndOfRoundTransition transition = FindFirstObjectByType<EndOfRoundTransition>();
        if (transition != null)
        {
            transition.nextSceneName = sceneToLoad;
            transition.TriggerEndSequence();
        }
        else
        {
            SceneManager.LoadSceneAsync(sceneToLoad);
        }

        currRoundActive = false;
        currRound = null;
    }

    // This calculates points perfectly and directly updates the GameManager
    private List<int> UpdateScoresAndCheckGameWin()
    {
        // 1. Find the highest score IN THIS ROUND
        int highestRoundScore = -1;
        for (int i = 0; i < currRoundScores.Length; i++)
        {
            if (currRoundScores[i] > highestRoundScore)
                highestRoundScore = currRoundScores[i];
        }

        // 2. Give a permanent point in GameManager to anyone who tied for highest score —
        // unless nobody actually scored (e.g. a simultaneous full wipe in Mountain), in which
        // case nobody should get a point just for tying at 0.
        if (GameManager.Instance != null && highestRoundScore > 0)
        {
            for (int i = 0; i < currRoundScores.Length; i++)
            {
                if (currRoundScores[i] == highestRoundScore && i < GameManager.Instance.playerTotalScores.Length)
                {
                    GameManager.Instance.playerTotalScores[i]++;
                }
            }
        }

        // 3. Check if anyone hit 3 points
        List<int> gameWinners = new List<int>();
        if (GameManager.Instance != null)
        {
            for (int i = 0; i < GameManager.Instance.playerTotalScores.Length; i++)
            {
                if (GameManager.Instance.playerTotalScores[i] >= maxScore)
                {
                    gameWinners.Add(i);
                }
            }
        }

        return gameWinners;
    }

    public void SetPlayersCanMove(bool value)
    {
        foreach (GameObject playerObj in playerObjects)
        {
            if (playerObj == null) continue;
            Playermovement player = playerObj.GetComponentInChildren<Playermovement>();
            if (player != null) player.canMove = value;
        }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        basketContainer = FindFirstObjectByType<BasketContainer>();
        sprinklerManager = FindFirstObjectByType<SprinklerManager>();
        RefreshUIReferences();
    }
}