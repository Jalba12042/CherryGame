using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections; // Needed for the Coroutine sequence

public class GameWinScript : MonoBehaviour
{
    public TMP_Text winnerText;
    public Button[] menuButtons;
    private int currentIndex = 0;

    private bool canMove = true;
    private float deadzone = 0.5f;

    [SerializeField] private string localSceneName;

    public static List<int> winningPlayers = new List<int>();

    [Header("Name Mapping")]
    public string[] availableNames;

    [Header("Award Show Stage")]
    public Animator stageAnimator; // Drag your AwardShowStage here in the Inspector!

    [Header("Audio Polish")]
    public AudioSource sfxSource;
    public AudioClip celebrateSound;
    public float musicFadeDuration = 2f;

    void Start()
    {
        // Snap time back to normal
        Time.timeScale = 1f;

        // Tell the gameplay music to fade out and die
        if (GameplayMusicManager.Instance != null)
        {
            GameplayMusicManager.Instance.FadeOutToShop(musicFadeDuration);
        }

        // Hide the winner text at the very beginning so it doesn't spoil the reveal!
        if (winnerText != null) winnerText.gameObject.SetActive(false);

        // Start the Award Show Sequence!
        StartCoroutine(AwardShowSequence());
    }

    private IEnumerator AwardShowSequence()
    {
        // 1. Wait a brief second for the scene to settle
        yield return new WaitForSeconds(1f);

        // 2. Tell the curtain to open!
        if (stageAnimator != null)
        {
            stageAnimator.Play("CurtainReveal"); // Make sure this matches your animation name exactly
        }

        // 3. Play the Celebrate Sound as the curtains move
        if (sfxSource != null && celebrateSound != null)
        {
            sfxSource.PlayOneShot(celebrateSound);
        }

        // 4. Wait for the curtains to finish opening (Adjust this time to match your animation length)
        yield return new WaitForSeconds(1.5f);

        // 5. Now calculate and reveal the winner!
        RevealWinner();
    }

    private void RevealWinner()
    {
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
                winnerText.gameObject.SetActive(true); // Turn text back on!
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
                winnerText.gameObject.SetActive(true); // Turn text back on!
            }
        }

        HighlightButton();
    }

    void Update()
    {
        if (menuButtons == null || menuButtons.Length == 0) return;

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
            menuButtons[currentIndex].onClick.Invoke();
    }

    void HighlightButton()
    {
        if (menuButtons == null) return;
        for (int i = 0; i < menuButtons.Length; i++)
        {
            ColorBlock colors = menuButtons[i].colors;
            colors.normalColor = (i == currentIndex) ? Color.yellow : Color.white;
            menuButtons[i].colors = colors;
        }
    }

    public void GoToLocal()
    {
        if (GameplayMusicManager.Instance != null)
        {
            Destroy(GameplayMusicManager.Instance.gameObject);
        }
        SceneManager.LoadScene(localSceneName);
    }
}