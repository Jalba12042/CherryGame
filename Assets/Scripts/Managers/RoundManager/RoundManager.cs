using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;

    [Header("Current Round Information")]
    public float currRoundProgress;
    public float currRoundDurationInSecs;
    public Round currRound;
    public bool currRoundActive;
    public int[] currRoundScores;
    public List<Round> roundList; // list of rounds we can cycle through

    [Header("PowerUp List")]
    public List<GameObject> powerUpsInRotation; // List of all powerups in rotation

    [Header("Flag to allow repeated rounds if we so choose")]
    [SerializeField] private bool allowRepeats; // flag to allow repeated rounds if we so choose

    [Header("Scene Names")]
    [SerializeField] private string shopSceneName;
    [SerializeField] private string controllerSceneName;
    [SerializeField] private string winSceneName;

    [SerializeField] private int startTimerInSeconds;
    [SerializeField] private GameObject playerPrefab;

    public GameObject[] playerObjects;
    public List<GameObject> powerupsInPlay;
    private PlayerSpawn currPlayerSpawn;
    private int currRoundIndex;
    private int startTimer;
    private bool roundSelected;

    public RenderTexture[] playerFaceRenderTextures;


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
        if (currRound == null && (SceneManager.GetActiveScene().name.Equals(controllerSceneName) || SceneManager.GetActiveScene().name.Equals(shopSceneName)))
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
                // set scores
                currRoundScores = currRound.ScoreCount();

                // stop round
                StopCoroutine(StartRound());

                List<int> winnerIndexes = checkWinIndexes();

                WinScript.winningPlayers = winnerIndexes; // set winning players
                SceneManager.LoadSceneAsync(winSceneName);

                //GameManager.Instance.currGameState = GameManager.GameState.Win;

                // set all values to defaults and change scene back to shop
                roundSelected = false;
                currRoundActive = false;
                currRound = null;
                GameManager.Instance.currGameState = GameManager.GameState.Win;
                //SceneManager.LoadSceneAsync(shopSceneName);
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
        // spawn players
        playerObjects = new GameObject[GameManager.Instance.playerCount];
        for (int playerSlot = 0; playerSlot < GameManager.Instance.playerCount; playerSlot++)
        {
            int assignedControllerIndex = GameManager.Instance.controllerAssignments[playerSlot];

            // Spawn player for this slot
            playerObjects[playerSlot] = Instantiate(
                playerPrefab,
                currPlayerSpawn.spawnPoints[playerSlot].position,
                Quaternion.identity
            );

            Playermovement player = playerObjects[playerSlot].GetComponentInChildren<Playermovement>();
            player.GetComponent<PlayerEscapeUI>().playerIndex = playerSlot;
            player.playerIndex = playerSlot;

            if (assignedControllerIndex >= 0 && assignedControllerIndex < UnityEngine.InputSystem.Gamepad.all.Count)
            {
                player.assignedGamepad = UnityEngine.InputSystem.Gamepad.all[assignedControllerIndex];
                Debug.Log($"Player {playerSlot + 1} using controller {assignedControllerIndex} (Player index in array: {playerSlot})");
            }
            else
            {
                Debug.LogWarning($"Player {playerSlot + 1} has no valid controller assigned!");
            }

            //Assigns player color
            PlayerColorAssigner colorAssigner = playerObjects[playerSlot].GetComponentInChildren<PlayerColorAssigner>();
            if (colorAssigner != null)
            {
                colorAssigner.AssignColor(playerSlot);
            }

            // Attach each player their own face cam
            Camera faceCam = player.GetComponentInChildren<Camera>();
            if (faceCam != null && playerSlot < playerFaceRenderTextures.Length)
                faceCam.targetTexture = playerFaceRenderTextures[playerSlot];
        }

        /*playerObjects = new GameObject[GameManager.Instance.playerCount];
        for (int i = 0; i < GameManager.Instance.playerCount; i++)
        {
            playerObjects[i] = Instantiate(playerPrefab, currPlayerSpawn.spawnPoints[i].position, Quaternion.identity);
            Playermovement player = playerObjects[i].GetComponentInChildren<Playermovement>();
            player.GetComponent<PlayerEscapeUI>().playerIndex = i;

            player.playerIndex = i;

            int assignedControllerIndex = GameManager.Instance.controllerAssignments[i];
            if (assignedControllerIndex >= 0 && assignedControllerIndex < UnityEngine.InputSystem.Gamepad.all.Count)
            {
                player.assignedGamepad = UnityEngine.InputSystem.Gamepad.all[assignedControllerIndex];
                Debug.Log($"Player {i + 1} using controller {assignedControllerIndex} (Player index in array: {i})");
            }
            else
            {
                Debug.LogWarning($"Player {i + 1} has no valid controller assigned!");
            }

            //Attaches each player their own face cam
            Camera faceCam = player.GetComponentInChildren<Camera>();
            if (faceCam != null && i < playerFaceRenderTextures.Length)
                faceCam.targetTexture = playerFaceRenderTextures[i];

        }*/

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
        return currWinnerIndexes;
    }
}
