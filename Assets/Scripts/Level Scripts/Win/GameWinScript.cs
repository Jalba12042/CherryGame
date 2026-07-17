using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

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
    public Animator stageAnimator;

    [Header("Audio Polish")]
    public AudioSource sfxSource;
    public AudioClip celebrateSound;
    public float musicFadeDuration = 2f;

    [Header("Award Show Music")]
    public AudioSource introMusicSource; // <--- NEW: Dedicated source for the intro song
    public float introMusicFadeSpeed = 1.5f; // <--- NEW: How fast it fades in and out

    void Start()
    {
        Time.timeScale = 1f;

        if (GameplayMusicManager.Instance != null)
        {
            GameplayMusicManager.Instance.FadeOutToShop(musicFadeDuration);
        }

        if (winnerText != null) winnerText.gameObject.SetActive(false);

        StartCoroutine(AwardShowSequence());
    }

    private IEnumerator AwardShowSequence()
    {
        // 1. Wait a brief second for the scene to settle
        yield return new WaitForSeconds(1f);

        // 2. Play the Award Show Intro Music INSTANTLY (No slow fade-in!)
        if (introMusicSource != null)
        {
            introMusicSource.volume = 1f; // Hit full volume immediately
            introMusicSource.Play();
        }

        // 3. Tell the curtain to open!
        if (stageAnimator != null)
        {
            stageAnimator.Play("CurtainReveal");
        }

        // 4. Play the Cheer/Celebrate Sound effect
        if (sfxSource != null && celebrateSound != null)
        {
            sfxSource.PlayOneShot(celebrateSound);
        }

        // 5. Wait for the curtains to finish opening
        yield return new WaitForSeconds(1.5f);

        // 6. Let the Intro Music bump for a few seconds while players look at the stage!
        yield return new WaitForSeconds(3.5f);

        // 7. Fade OUT the Intro Music to build tension before the envelopes
        if (introMusicSource != null)
        {
            StartCoroutine(FadeAudio(introMusicSource, 0f, introMusicFadeSpeed));
        }

        // 8. Wait for the music to fade out, plus one second of silence
        yield return new WaitForSeconds(introMusicFadeSpeed + 1f);

        // 9. Now calculate and reveal the winner! 
        RevealWinner();
    }

    // ==========================================
    // --- THE SMOOTH AUDIO FADER TOOL ---
    // ==========================================
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

        // If we faded it to 0, stop playing the track completely to save memory
        if (targetVolume == 0f)
        {
            audioSource.Stop();
        }
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
    }

    void Update()
    {
        if (menuButtons == null || menuButtons.Length == 0) return;

        // BULLETPROOF FIX 1: Make sure the InputManager actually exists before asking it for controls
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
            // Make sure the button isn't missing before trying to click it
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
            // BULLETPROOF FIX 2: Skip any empty slots in the Inspector so the game doesn't crash
            if (menuButtons[i] == null) continue;

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