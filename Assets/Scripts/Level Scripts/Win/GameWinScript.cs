using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameWinScript : MonoBehaviour
{
    [Header("Final UI Board")]
    public GameObject finalBoardPanel; // <-- Drag your Play Again/Quit panel here!
    public TMP_Text winnerText;
    public Button[] menuButtons;
    private int currentIndex = 0;
    private bool canMove = true;
    private float deadzone = 0.5f;
    private bool isMenuActive = false; // Locks the menu until the puppets finish!

    [SerializeField] private string localSceneName;

    public static List<int> winningPlayers = new List<int>();

    [Header("Name & Color Mapping")]
    public string[] availableNames;
    public Sprite[] colorIcons; // <-- Drag your 8 colored heads here

    [Header("Award Show Stage")]
    public Animator stageAnimator;

    [Header("Puppet Audio")]
    public AudioClip drumrollClip;
    public AudioClip sadClip;

    [Header("Award Show Music & SFX")]
    public AudioSource introMusicSource;
    public AudioSource sfxSource;
    public AudioClip celebrateSound;
    public float musicFadeDuration = 2f;
    public float introMusicFadeSpeed = 1.5f;

    [Header("Puppet Prefabs")]
    public GameObject firstPlacePrefab;
    public GameObject secondPlacePrefab;
    public GameObject thirdPlacePrefab;
    public GameObject fourthPlacePrefab;

    [Header("Puppet Anchors")]
    public Transform anchor1st;
    public Transform anchor2nd;
    public Transform anchor3rd;
    public Transform anchor4th;

    private class PlayerRank
    {
        public int playerID;
        public int score;
        public int calculatedRank;
    }

    void Start()
    {
        Time.timeScale = 1f;

        // Hide the final menu board so it doesn't ruin the surprise
        if (finalBoardPanel != null) finalBoardPanel.SetActive(false);
        if (winnerText != null) winnerText.gameObject.SetActive(false);

        if (GameplayMusicManager.Instance != null)
        {
            GameplayMusicManager.Instance.FadeOutToShop(musicFadeDuration);
        }

        StartCoroutine(AwardShowSequence());
    }

    private IEnumerator AwardShowSequence()
    {
        // 1. Wait a brief second for the scene to settle
        yield return new WaitForSeconds(1f);

        // 2. Play the Award Show Intro Music INSTANTLY
        if (introMusicSource != null)
        {
            introMusicSource.volume = 1f;
            introMusicSource.Play();
        }

        // 3. Tell the curtain to open!
        if (stageAnimator != null)
        {
            stageAnimator.Play("CurtainsIntro"); // Updated to match your animation name!
        }

        // 4. Wait for the curtains to finish opening
        yield return new WaitForSeconds(1.5f);

        // 5. Let the Intro Music bump for a few seconds
        yield return new WaitForSeconds(3.5f);

        // 6. Fade OUT the Intro Music to build tension
        if (introMusicSource != null)
        {
            StartCoroutine(FadeAudio(introMusicSource, 0f, introMusicFadeSpeed));
        }

        // 7. Wait for the music to fade out, plus one second of silence
        yield return new WaitForSeconds(introMusicFadeSpeed + 1f);

        // 8. Start the Drumrolls and Puppet drop!
        yield return StartCoroutine(RevealPuppetsRoutine());

        // 9. Show the Final Board and activate the menu!
        RevealWinnerBoard();
    }

    private IEnumerator RevealPuppetsRoutine()
    {
        List<PlayerRank> rankings = new List<PlayerRank>();
        int totalPlayers = 4;

        if (GameManager.Instance != null)
        {
            totalPlayers = GameManager.Instance.playerCount;
            for (int i = 0; i < totalPlayers; i++)
            {
                rankings.Add(new PlayerRank { playerID = i, score = GameManager.Instance.playerTotalScores[i] });
            }
        }
        else
        {
            for (int i = 0; i < 4; i++) rankings.Add(new PlayerRank { playerID = i, score = Random.Range(0, 4) });
        }

        // Sort Highest to Lowest
        rankings.Sort((p1, p2) => p2.score.CompareTo(p1.score));

        // Calculate actual ranks (so ties are handled perfectly)
        rankings[0].calculatedRank = 1;
        for (int i = 1; i < rankings.Count; i++)
        {
            if (rankings[i].score == rankings[i - 1].score)
                rankings[i].calculatedRank = rankings[i - 1].calculatedRank; // Tie!
            else
                rankings[i].calculatedRank = i + 1; // Normal placement
        }

        // Loop backwards to reveal 4th, then 3rd, 2nd, 1st
        for (int i = rankings.Count - 1; i >= 0; i--)
        {
            int rank = rankings[i].calculatedRank;
            PlayerRank pData = rankings[i];

            if (sfxSource != null && drumrollClip != null) sfxSource.PlayOneShot(drumrollClip);

            yield return new WaitForSeconds(1.5f); // Tension building!

            GameObject prefabToSpawn = null;
            Transform targetAnchor = null;
            AudioClip reactionClip = celebrateSound;

            if (rank >= 4) { prefabToSpawn = fourthPlacePrefab; targetAnchor = anchor4th; reactionClip = sadClip; }
            else if (rank == 3) { prefabToSpawn = thirdPlacePrefab; targetAnchor = anchor3rd; }
            else if (rank == 2) { prefabToSpawn = secondPlacePrefab; targetAnchor = anchor2nd; }
            else if (rank == 1) { prefabToSpawn = firstPlacePrefab; targetAnchor = anchor1st; }

            if (prefabToSpawn != null && targetAnchor != null)
            {
                GameObject spawnedPuppet = Instantiate(prefabToSpawn, targetAnchor.position, targetAnchor.rotation, targetAnchor);

                Transform headTransform = spawnedPuppet.transform.Find("PlayerHead");
                if (headTransform != null)
                {
                    Image headImage = headTransform.GetComponent<Image>();
                    if (headImage != null && GameManager.Instance != null && GameManager.Instance.playerCustomizations.Count > pData.playerID)
                    {
                        int colorIndex = GameManager.Instance.playerCustomizations[pData.playerID].colorIndex;
                        if (colorIndex >= 0 && colorIndex < colorIcons.Length)
                        {
                            headImage.sprite = colorIcons[colorIndex];
                        }
                    }
                }

                Animator puppetAnim = spawnedPuppet.GetComponent<Animator>();
                if (puppetAnim != null) puppetAnim.Play("PuppetSlideUp");
            }

            if (sfxSource != null && reactionClip != null) sfxSource.PlayOneShot(reactionClip);

            yield return new WaitForSeconds(2.0f); // Let them look at the puppet before the next drumroll
        }
    }

    private void RevealWinnerBoard()
    {
        // Turn on the final board UI
        if (finalBoardPanel != null) finalBoardPanel.SetActive(true);

        if (winningPlayers == null || winningPlayers.Count == 0)
        {
            winningPlayers = new List<int> { 0 };
        }

        bool isTie = winningPlayers.Count > 1;

        if (isTie)
        {
            string winnersString = "";
            for (int i = 0; i < winningPlayers.Count; i++)
            {
                int pID = winningPlayers[i];
                string pName = "Player " + (pID + 1);

                if (GameManager.Instance != null && GameManager.Instance.playerCustomizations.Count > pID)
                {
                    int nameIdx = GameManager.Instance.playerCustomizations[pID].nameIndex;
                    if (availableNames != null && nameIdx >= 0 && nameIdx < availableNames.Length)
                    {
                        pName = availableNames[nameIdx];
                    }
                }

                winnersString += pName;
                if (i == winningPlayers.Count - 2) winnersString += " & ";
                else if (i < winningPlayers.Count - 1) winnersString += ", ";
            }
            winnersString += " TIED!";

            if (winnerText != null)
            {
                winnerText.text = winnersString.ToUpper();
                winnerText.gameObject.SetActive(true);
            }
        }
        else
        {
            int winnerID = winningPlayers[0];
            string winName = "PLAYER " + (winnerID + 1);

            if (GameManager.Instance != null && GameManager.Instance.playerCustomizations.Count > winnerID)
            {
                var data = GameManager.Instance.playerCustomizations[winnerID];
                if (availableNames != null && data.nameIndex >= 0 && data.nameIndex < availableNames.Length)
                {
                    winName = availableNames[data.nameIndex];
                }
            }

            if (winnerText != null)
            {
                winnerText.text = winName.ToUpper() + " WINS THE GAME!";
                winnerText.gameObject.SetActive(true);
            }
        }

        HighlightButton();
        isMenuActive = true; // Unlock the controllers!
    }

    void Update()
    {
        // Stop players from moving the menu while the puppets are still popping up!
        if (!isMenuActive || menuButtons == null || menuButtons.Length == 0) return;

        if (InputManager.Instance == null) return;

        Vector2 move = InputManager.Instance.GetMove(1);

        if (canMove)
        {
            if (move.y > deadzone)
            {
                currentIndex = Mathf.Max(0, currentIndex - 1);
                HighlightButton();
                canMove = false;
            }
            else if (move.y < -deadzone)
            {
                currentIndex = Mathf.Min(menuButtons.Length - 1, currentIndex + 1);
                HighlightButton();
                canMove = false;
            }
        }

        if (Mathf.Abs(move.y) < 0.2f) canMove = true;

        if (InputManager.Instance.GetConfirmDown(1))
        {
            if (menuButtons[currentIndex] != null)
            {
                menuButtons[currentIndex].onClick.Invoke();
            }
        }
    }

    void HighlightButton()
    {
        if (menuButtons == null) return;

        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] == null) continue;

            ColorBlock colors = menuButtons[i].colors;
            colors.normalColor = (i == currentIndex) ? Color.yellow : Color.white;
            menuButtons[i].colors = colors;
        }
    }

    private IEnumerator FadeAudio(AudioSource audioSource, float targetVolume, float duration)
    {
        float currentTime = 0;
        float startVolume = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;
        if (targetVolume == 0f) audioSource.Stop();
    }

    public void GoToLocal()
    {
        if (GameplayMusicManager.Instance != null) Destroy(GameplayMusicManager.Instance.gameObject);
        SceneManager.LoadScene(localSceneName);
    }
}