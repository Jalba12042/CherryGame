using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
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
    private int currRoundIndex;

    public RenderTexture[] playerFaceRenderTextures;
    public int[] roundsWon = { 0, 0, 0, 0 };

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
        GameObject timerGO = GameObject.FindWithTag("Timer");
        if (timerGO != null) timerText = timerGO.GetComponent<TextMeshProUGUI>();

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

    // Online: true for local/offline play too, since NetworkManager isn't running at all then.
    private bool IsOnline => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
    private bool IsOnlineServer => IsOnline && NetworkManager.Singleton.IsServer;

    public void switchRoundScene()
    {
        if (SceneManager.GetActiveScene().name.Equals(currRound.sceneName)) return;

        if (IsOnline)
        {
            // Non-host clients just wait: the host's load propagates to everyone via
            // NetworkManager.SceneManager.
            if (IsOnlineServer)
                NetworkManager.Singleton.SceneManager.LoadScene(currRound.sceneName, LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadSceneAsync(currRound.sceneName);
        }
    }

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

        // Only the server (or local/offline play) actually drives the timer/goal-spawning
        // coroutines - online clients just mirror OnlineRoundSync's replicated state instead,
        // otherwise every machine would run its own independent, unsynced timer.
        if (IsOnline && !IsOnlineServer)
            StartCoroutine(FollowOnlineRound());
        else
            StartCoroutine(StartTimer());
    }

    private IEnumerator FollowOnlineRound()
    {
        while (OnlineRoundSync.Instance == null) yield return null;
        OnlineRoundSync sync = OnlineRoundSync.Instance;

        RefreshUIReferences();
        if (timerBackgroundUI != null) timerBackgroundUI.SetActive(true);
        if (timerText != null) timerText.text = "";
        SetPlayersCanMove(false);

        bool movementUnlocked = false;
        while (true)
        {
            currRoundDurationInSecs = sync.RoundDuration.Value;
            currRoundProgress = sync.RoundProgress.Value;
            currRoundProgressNormalized = currRoundDurationInSecs > 0 ? currRoundProgress / currRoundDurationInSecs : 0f;
            currRoundActive = sync.RoundActive.Value;

            currRoundScores = new int[sync.Scores.Count];
            for (int i = 0; i < sync.Scores.Count; i++) currRoundScores[i] = sync.Scores[i];

            if (timerText != null && currRoundActive)
                timerText.text = Mathf.CeilToInt(currRoundDurationInSecs - currRoundProgress).ToString();

            if (sync.PlayersCanMove.Value && !movementUnlocked)
            {
                movementUnlocked = true;
                SetPlayersCanMove(true);
            }

            yield return null;
        }
    }

    private void SpawnPlayers()
    {
        currPlayerSpawn = FindFirstObjectByType<PlayerSpawn>();

        // Online: players were already spawned server-side by NetworkPlayerSpawner (so every
        // client sees the same set, with correct ownership). Just adopt them into the usual
        // bookkeeping instead of instantiating new ones.
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            AdoptOnlinePlayers();
            return;
        }

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

    private void AdoptOnlinePlayers()
    {
        Playermovement[] allPlayers = FindObjectsByType<Playermovement>(FindObjectsSortMode.None)
            .OrderBy(p => p.GlobalIndex.Value)
            .ToArray();

        playerObjects = new GameObject[allPlayers.Length];

        // BasketContainer.Awake() hid baskets based on this machine's *local* playerCount,
        // which online is wrong (it doesn't know about other clients' players yet at that
        // point). Correct it now that we know the real total.
        if (basketContainer != null)
        {
            basketContainer.onlinePlayerCountOverride = allPlayers.Length;
            for (int i = 0; i < basketContainer.baskets.Count; i++)
                basketContainer.baskets[i].SetActive(i < allPlayers.Length);
        }

        for (int i = 0; i < allPlayers.Length; i++)
        {
            Playermovement player = allPlayers[i];
            int globalIndex = player.GlobalIndex.Value;
            if (globalIndex < 0 || globalIndex >= allPlayers.Length) continue;

            GameObject playerObj = player.gameObject;
            playerObjects[globalIndex] = playerObj;
            player.initialSpawnPosition = playerObj.transform.position;

            var customization = playerObj.GetComponentInChildren<PlayerCustomization>();
            if (customization != null)
            {
                customization.playerIndex = globalIndex;

                if (basketContainer != null && globalIndex < basketContainer.baskets.Count)
                {
                    GameObject basketObj = basketContainer.baskets[globalIndex];
                    BasketColorSync basket = basketObj.GetComponentInChildren<BasketColorSync>();
                    if (basket != null) basket.SetColor(customization.NetworkCustomization.Value.colorIndex);
                }
            }

            PlayerEscapeUI escapeUI = playerObj.GetComponent<PlayerEscapeUI>();
            if (escapeUI != null) escapeUI.playerIndex = globalIndex;

            Camera faceCam = player.GetComponentInChildren<Camera>();
            if (faceCam != null && globalIndex < playerFaceRenderTextures.Length)
                faceCam.targetTexture = playerFaceRenderTextures[globalIndex];
        }

        // Local input (playerIndex/playerID/gamepad) only matters for players THIS machine
        // owns, remapped back to local gamepad slots 0..N-1 in the order they were
        // registered (GlobalIndex increases monotonically within one client's block).
        Playermovement[] ownedPlayers = allPlayers.Where(p => p.IsOwner).OrderBy(p => p.GlobalIndex.Value).ToArray();
        for (int i = 0; i < ownedPlayers.Length; i++)
        {
            ownedPlayers[i].playerIndex = i;
            ownedPlayers[i].playerID = i + 1;

            Gamepad assignedGamepad = GameManager.Instance.GetAssignedGamepad(i);
            if (assignedGamepad != null) ownedPlayers[i].assignedGamepad = assignedGamepad;
        }
    }

    public IEnumerator StartTimer()
    {
        // 1. Double check UI is attached
        RefreshUIReferences();

        // 2. --- THE FIX: Reveal the Cardboard UI IMMEDIATELY! ---
        if (timerBackgroundUI != null)
        {
            timerBackgroundUI.SetActive(true);
            Animator bgAnim = timerBackgroundUI.GetComponent<Animator>();
            if (bgAnim != null) bgAnim.SetTrigger("StartTimer");
        }

        TimerUIManager tManager = FindFirstObjectByType<TimerUIManager>();
        if (tManager != null) tManager.RevealTimer();

        // 3. Keep the actual timer clock numbers empty so it doesn't count early
        if (timerText != null) timerText.text = "";

        SetPlayersCanMove(false);

        // 4. Run the 3-2-1 Screen Text Loop
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

        // 5. GO! Now allow movement and start the real background clock
        SetPlayersCanMove(true);
        currRoundActive = true;

        if (IsOnlineServer)
        {
            OnlineRoundSync.Instance.RoundDuration.Value = currRoundDurationInSecs;
            OnlineRoundSync.Instance.RoundActive.Value = true;
            OnlineRoundSync.Instance.PlayersCanMove.Value = true;
        }

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

            if (IsOnlineServer) OnlineRoundSync.Instance.RoundProgress.Value = currRoundProgress;

            currRound.RoundUpdate();
            yield return null;
        }

        currRoundProgress = currRoundDurationInSecs;
        currRoundProgressNormalized = 1f;

        currRoundScores = currRound.ScoreCount();
        WinScript.winningPlayers = checkWinIndexes();

        List<int> gameWinners = checkGameWinIndexes();

        if (IsOnlineServer)
        {
            OnlineRoundSync.Instance.SetScores(currRoundScores);
            OnlineRoundSync.Instance.RoundActive.Value = false;
        }

        if (gameWinners.Count != 0)
        {
            GameWinScript.winningPlayers = gameWinners;
            roundsWon = new int[] { 0, 0, 0, 0 };

            if (IsOnlineServer) NetworkManager.Singleton.SceneManager.LoadScene(gameWinSceneName, LoadSceneMode.Single);
            else if (!IsOnline) SceneManager.LoadSceneAsync(gameWinSceneName);
        }
        else
        {
            if (IsOnlineServer) NetworkManager.Singleton.SceneManager.LoadScene(winSceneName, LoadSceneMode.Single);
            else if (!IsOnline) SceneManager.LoadSceneAsync(winSceneName);
        }

        currRoundActive = false;
        currRound = null;
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