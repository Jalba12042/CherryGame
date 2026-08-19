using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameWinScript : MonoBehaviour
{
    [Header("UI & Stage")]
    public Animator stageAnimator;
    public TMP_Text winText;
    [Tooltip("The exact name of the animation that opens the curtains")]
    public string curtainsOpenAnimName = "CurtainsOpen";

    [Header("--- Main Award Sign (CINEMA Board) ---")]
    [Tooltip("Drag the big AwardSign object here!")]
    public Animator mainAwardSignAnimator;
    [Tooltip("How long does the CINEMA sign's entire animation take?")]
    public float awardSignFullDuration = 8.5f;

    [Header("--- Award Show Music ---")]
    [Tooltip("Drag your AudioSound object with the music here!")]
    public AudioSource awardShowMusic;
    [Tooltip("How many seconds it takes to fade the music out to silence")]
    public float musicFadeDuration = 1.5f;

    [Header("--- DEMO Marquee Sign & Puppet Outros ---")]
    [Tooltip("Drag your 1stplaceSigns object here")]
    public Animator marqueeAnimator;
    [Tooltip("The exact name of the sign's outro animation state")]
    public string signOutroAnimName = "1stplaceoutro";
    [Tooltip("The exact name of the puppet's outro animation state")]
    public string puppetOutroAnimName = "1splaceoutro";
    [Tooltip("How long to wait for the sign/puppet to slide out before the curtains open")]
    public float signOutroDuration = 1.0f;
    [Tooltip("Drag the Image component from 1stplaceSigns here")]
    public Image marqueeImage;
    [Tooltip("How long to wait after the puppet pops up before showing buttons")]
    public float postRevealWaitTime = 3f;

    [Header("Marquee Sprites (To swap on the single board)")]
    public Sprite sign1stPlace;
    public Sprite sign2ndPlace;
    public Sprite sign3rdPlace;
    public Sprite sign4thPlace;

    [Header("1st Place Setup")]
    public GameObject firstPlacePuppet;
    public Sprite[] colorSprites1stPlace;

    [Header("2nd Place Setup")]
    public GameObject secondPlacePuppet;
    public Sprite[] colorSprites2ndPlace;

    [Header("3rd Place Setup")]
    public GameObject thirdPlacePuppet;
    public Sprite[] colorSprites3rdPlace;

    [Header("4th Place Setup")]
    public GameObject fourthPlacePuppet;
    public Sprite[] colorSprites4thPlace;

    [Header("Last Place (Sad) Setup")]
    public Sprite[] colorSpritesLastPlace;

    [Header("Audio Polish")]
    public AudioSource drumrollAudio;
    public float drumrollWaitTime = 2f;

    [Header("Crowd Audio Polish")]
    public AudioSource crowdAudioSource;
    public AudioClip cheerSound;
    public AudioClip booSound;

    [Header("--- Post-Game Options (Images) ---")]
    public GameObject playAgainImage;
    public GameObject leaveImage;
    public string playAgainSceneName = "ControllerConnectScene";

    [Header("--- Leave Transition (Background Swap & Paper) ---")]
    public string leaveSceneName = "Title Screen";
    public GameObject redPanelBackground;
    public GameObject skyMainMenuBackground;
    public Animator paperTransitionAnimator;
    public string paperReverseTriggerName = "PaperReverseTrigger";
    public float paperAnimationDuration = 1.0f;

    private bool canSelectPostGame = false;
    private bool isTransitioning = false;

    // --- NEW: Controller Navigation Variables ---
    private int postGameSelectedIndex = 0; // 0 = Play Again, 1 = Leave
    private float stickCooldown = 0f;

    void Awake()
    {
        if (marqueeAnimator != null) marqueeAnimator.gameObject.SetActive(false);
    }

    void Start()
    {
        if (winText != null) winText.gameObject.SetActive(false);
        if (firstPlacePuppet != null) firstPlacePuppet.SetActive(false);
        if (secondPlacePuppet != null) secondPlacePuppet.SetActive(false);
        if (thirdPlacePuppet != null) thirdPlacePuppet.SetActive(false);
        if (fourthPlacePuppet != null) fourthPlacePuppet.SetActive(false);

        if (playAgainImage != null) playAgainImage.SetActive(false);
        if (leaveImage != null) leaveImage.SetActive(false);

        if (mainAwardSignAnimator != null) mainAwardSignAnimator.gameObject.SetActive(false);

        if (redPanelBackground != null) redPanelBackground.SetActive(true);
        if (skyMainMenuBackground != null) skyMainMenuBackground.SetActive(false);

        StartCoroutine(AwardSequence());
    }

    // --- NEW: Handle Controller Input for Post-Game Buttons ---
    void Update()
    {
        if (canSelectPostGame && !isTransitioning)
        {
            float moveX = 0f;
            bool confirmPressed = false;

            // Keyboard support
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame) moveX = -1f;
                if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame) moveX = 1f;
                if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame) confirmPressed = true;
            }

            // Gamepad support (Any connected gamepad can navigate)
            foreach (var pad in Gamepad.all)
            {
                if (pad.dpad.left.wasPressedThisFrame) moveX = -1f;
                if (pad.dpad.right.wasPressedThisFrame) moveX = 1f;

                // Stick support with a small cooldown so it doesn't flicker wildly
                if (Time.unscaledTime > stickCooldown)
                {
                    if (pad.leftStick.ReadValue().x < -0.5f) { moveX = -1f; stickCooldown = Time.unscaledTime + 0.2f; }
                    if (pad.leftStick.ReadValue().x > 0.5f) { moveX = 1f; stickCooldown = Time.unscaledTime + 0.2f; }
                }

                if (pad.buttonSouth.wasPressedThisFrame) confirmPressed = true;
            }

            // Apply navigation logic
            if (moveX < -0.1f)
            {
                postGameSelectedIndex = 0; // Play Again
                UpdatePostGameHighlight();
            }
            else if (moveX > 0.1f)
            {
                postGameSelectedIndex = 1; // Leave
                UpdatePostGameHighlight();
            }

            // Execute button
            if (confirmPressed)
            {
                if (postGameSelectedIndex == 0) SelectPlayAgain();
                else SelectLeave();
            }
        }
    }

    private void UpdatePostGameHighlight()
    {
        // Visually scale and tint the selected option Yellow
        if (playAgainImage != null)
        {
            playAgainImage.transform.localScale = (postGameSelectedIndex == 0) ? new Vector3(1.1f, 1.1f, 1.1f) : Vector3.one;
            Image img = playAgainImage.GetComponent<Image>();
            if (img != null) img.color = (postGameSelectedIndex == 0) ? Color.yellow : Color.white;
        }

        if (leaveImage != null)
        {
            leaveImage.transform.localScale = (postGameSelectedIndex == 1) ? new Vector3(1.1f, 1.1f, 1.1f) : Vector3.one;
            Image img = leaveImage.GetComponent<Image>();
            if (img != null) img.color = (postGameSelectedIndex == 1) ? Color.yellow : Color.white;
        }
    }

    private IEnumerator AwardSequence()
    {
        // 1. Drop the curtains immediately
        if (stageAnimator != null) stageAnimator.Play("CurtainsClose");
        yield return new WaitForSeconds(1.5f);

        // 2. Turn on the big CINEMA sign AND play the music!
        if (mainAwardSignAnimator != null) mainAwardSignAnimator.gameObject.SetActive(true);
        if (awardShowMusic != null) awardShowMusic.Play();

        // 3. Wait for the CINEMA sign animation, but stop early to start the audio fade!
        float initialWait = Mathf.Max(0, awardSignFullDuration - musicFadeDuration);
        yield return new WaitForSeconds(initialWait);

        // Start fading the music down to 0 volume
        if (awardShowMusic != null) StartCoroutine(FadeAudio(awardShowMusic, 0f, musicFadeDuration));

        // Wait for the remainder of the sign's animation
        yield return new WaitForSeconds(Mathf.Min(musicFadeDuration, awardSignFullDuration));

        // 4. Turn the CINEMA sign OFF completely so the screen is clear!
        if (mainAwardSignAnimator != null) mainAwardSignAnimator.gameObject.SetActive(false);

        // 5. Start sorting and revealing players
        List<int> sortedPlayers = new List<int>();
        int pCount = 0;
        if (GameManager.Instance != null)
        {
            pCount = GameManager.Instance.playerCount;
            for (int i = 0; i < pCount; i++) sortedPlayers.Add(i);
            sortedPlayers.Sort((p1, p2) => GameManager.Instance.playerTotalScores[p2].CompareTo(GameManager.Instance.playerTotalScores[p1]));
        }

        int winnerID = sortedPlayers.Count > 0 ? sortedPlayers[0] : 0;
        int secondPlaceID = sortedPlayers.Count > 1 ? sortedPlayers[1] : 0;
        int thirdPlaceID = sortedPlayers.Count > 2 ? sortedPlayers[2] : 0;
        int fourthPlaceID = sortedPlayers.Count > 3 ? sortedPlayers[3] : 0;

        if (pCount >= 4) yield return StartCoroutine(RevealRoutine(sign4thPlace, fourthPlacePuppet, fourthPlaceID, pCount == 4));
        if (pCount >= 3) yield return StartCoroutine(RevealRoutine(sign3rdPlace, thirdPlacePuppet, thirdPlaceID, pCount == 3));
        if (pCount >= 2) yield return StartCoroutine(RevealRoutine(sign2ndPlace, secondPlacePuppet, secondPlaceID, pCount == 2));
        yield return StartCoroutine(RevealRoutine(sign1stPlace, firstPlacePuppet, winnerID, false, true));
    }

    private IEnumerator RevealRoutine(Sprite signSprite, GameObject puppetObj, int playerID, bool isSadLoser, bool isWinner = false)
    {
        if (marqueeImage != null) marqueeImage.sprite = signSprite;
        if (marqueeAnimator != null) marqueeAnimator.gameObject.SetActive(true);

        if (drumrollAudio != null) drumrollAudio.Play();
        yield return new WaitForSeconds(drumrollWaitTime);

        if (puppetObj != null)
        {
            puppetObj.SetActive(true);
            Image puppetImg = puppetObj.GetComponent<Image>();
            Animator puppetAnim = puppetObj.GetComponent<Animator>();

            if (puppetImg != null && GameManager.Instance != null)
            {
                int colorIndex = GameManager.Instance.playerCustomizations[playerID].colorIndex;
                Sprite[] targetArray;

                if (isWinner) targetArray = colorSprites1stPlace;
                else if (isSadLoser) targetArray = colorSpritesLastPlace;
                else
                {
                    if (puppetObj == secondPlacePuppet) targetArray = colorSprites2ndPlace;
                    else if (puppetObj == thirdPlacePuppet) targetArray = colorSprites3rdPlace;
                    else targetArray = colorSprites4thPlace;
                }

                if (colorIndex < targetArray.Length) puppetImg.sprite = targetArray[colorIndex];
            }

            if (isSadLoser)
            {
                if (puppetAnim != null) puppetAnim.Play(puppetObj == secondPlacePuppet ? "PuppetSad_2ndPlace" : puppetObj == thirdPlacePuppet ? "PuppetSad_3rdPlace" : "PuppetSad_4thPlace");
                if (crowdAudioSource != null && booSound != null) crowdAudioSource.PlayOneShot(booSound);
            }
            else
            {
                if (puppetAnim != null) puppetAnim.Play(isWinner ? "PuppetSlideUp_1stPlace" : puppetObj == secondPlacePuppet ? "PuppetSlideUp_2ndPlace" : puppetObj == thirdPlacePuppet ? "PuppetSlideUp_3rdPlace" : "PuppetSlideUp_4thPlace");
                if (crowdAudioSource != null && cheerSound != null) crowdAudioSource.PlayOneShot(cheerSound);
            }
        }

        if (winText != null)
        {
            if (isWinner) winText.text = "1ST PLACE!";
            else if (isSadLoser) winText.text = "LAST PLACE...";
            else winText.text = puppetObj == secondPlacePuppet ? "2ND PLACE!" : puppetObj == thirdPlacePuppet ? "3RD PLACE!" : "4TH PLACE!";
            winText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(postRevealWaitTime);

        if (!isWinner)
        {
            if (marqueeAnimator != null) marqueeAnimator.Play(signOutroAnimName);
            yield return new WaitForSeconds(signOutroDuration);

            if (marqueeAnimator != null) marqueeAnimator.gameObject.SetActive(false);
            if (puppetObj != null) puppetObj.SetActive(false);
            if (winText != null) winText.gameObject.SetActive(false);
        }
        else
        {
            if (playAgainImage != null) playAgainImage.SetActive(true);
            if (leaveImage != null) leaveImage.SetActive(true);

            // --- NEW: Trigger the visual highlight the moment the buttons appear ---
            postGameSelectedIndex = 0;
            UpdatePostGameHighlight();

            canSelectPostGame = true;
        }
    }

    public void SelectPlayAgain()
    {
        if (!canSelectPostGame || isTransitioning) return;
        StartCoroutine(PlayAgainTransition());
    }

    public void SelectLeave()
    {
        if (!canSelectPostGame || isTransitioning) return;
        StartCoroutine(LeaveTransition());
    }

    private IEnumerator PlayAgainTransition()
    {
        isTransitioning = true;

        // Safety fade-out if the music is still playing somehow
        if (awardShowMusic != null && awardShowMusic.isPlaying) StartCoroutine(FadeAudio(awardShowMusic, 0f, signOutroDuration));

        if (playAgainImage != null) playAgainImage.SetActive(false);
        if (leaveImage != null) leaveImage.SetActive(false);
        if (winText != null) winText.gameObject.SetActive(false);

        if (marqueeAnimator != null) marqueeAnimator.Play(signOutroAnimName);

        Animator firstPuppetAnim = firstPlacePuppet != null ? firstPlacePuppet.GetComponent<Animator>() : null;
        if (firstPuppetAnim != null) firstPuppetAnim.Play(puppetOutroAnimName);

        yield return new WaitForSeconds(signOutroDuration);

        if (firstPlacePuppet != null) firstPlacePuppet.SetActive(false);
        if (marqueeAnimator != null) marqueeAnimator.gameObject.SetActive(false);

        if (stageAnimator != null) stageAnimator.Play(curtainsOpenAnimName);
        yield return new WaitForSeconds(1.5f);

        if (!string.IsNullOrEmpty(playAgainSceneName))
        {
            SceneManager.LoadSceneAsync(playAgainSceneName);
        }
    }

    private IEnumerator LeaveTransition()
    {
        isTransitioning = true;

        // Safety fade-out if the music is still playing somehow
        if (awardShowMusic != null && awardShowMusic.isPlaying) StartCoroutine(FadeAudio(awardShowMusic, 0f, signOutroDuration));

        if (playAgainImage != null) playAgainImage.SetActive(false);
        if (leaveImage != null) leaveImage.SetActive(false);
        if (winText != null) winText.gameObject.SetActive(false);

        if (marqueeAnimator != null) marqueeAnimator.Play(signOutroAnimName);

        Animator firstPuppetAnim = firstPlacePuppet != null ? firstPlacePuppet.GetComponent<Animator>() : null;
        if (firstPuppetAnim != null) firstPuppetAnim.Play(puppetOutroAnimName);

        yield return new WaitForSeconds(signOutroDuration);

        if (firstPlacePuppet != null) firstPlacePuppet.SetActive(false);
        if (marqueeAnimator != null) marqueeAnimator.gameObject.SetActive(false);

        if (stageAnimator != null) stageAnimator.Play(curtainsOpenAnimName);

        yield return new WaitForSeconds(1.5f);

        if (redPanelBackground != null) redPanelBackground.SetActive(false);
        if (skyMainMenuBackground != null) skyMainMenuBackground.SetActive(true);

        if (paperTransitionAnimator != null)
        {
            paperTransitionAnimator.gameObject.SetActive(true);

            Image paperImg = paperTransitionAnimator.GetComponent<Image>();
            if (paperImg != null)
            {
                Color c = paperImg.color;
                c.a = 1f;
                paperImg.color = c;
            }

            if (!string.IsNullOrEmpty(paperReverseTriggerName))
            {
                paperTransitionAnimator.SetTrigger(paperReverseTriggerName);
            }

            paperTransitionAnimator.Update(0f);
            yield return new WaitForSeconds(paperAnimationDuration);
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }

        if (!string.IsNullOrEmpty(leaveSceneName))
        {
            SceneManager.LoadSceneAsync(leaveSceneName);
        }
    }

    private IEnumerator FadeAudio(AudioSource source, float targetVol, float duration)
    {
        float startVol = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVol, targetVol, elapsed / duration);
            yield return null;
        }

        source.volume = targetVol;
        if (targetVol == 0f) source.Stop();
    }
}