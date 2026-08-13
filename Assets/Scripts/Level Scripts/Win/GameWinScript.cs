using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameWinScript : MonoBehaviour
{
    [Header("UI & Stage")]
    public Animator stageAnimator;
    public TMP_Text winText;

    [Header("--- DEMO Marquee Sign (Single Object) ---")]
    [Tooltip("Drag your 1stplaceSigns object here")]
    public Animator marqueeAnimator;
    [Tooltip("Drag the Image component from 1stplaceSigns here")]
    public Image marqueeImage;
    [Tooltip("How long to wait after the puppet pops up before playing the Outro")]
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

    void Start()
    {
        if (winText != null) winText.gameObject.SetActive(false);
        if (firstPlacePuppet != null) firstPlacePuppet.SetActive(false);
        if (secondPlacePuppet != null) secondPlacePuppet.SetActive(false);
        if (thirdPlacePuppet != null) thirdPlacePuppet.SetActive(false);
        if (fourthPlacePuppet != null) fourthPlacePuppet.SetActive(false);

        // Ensure the single sign starts turned off
        if (marqueeAnimator != null) marqueeAnimator.gameObject.SetActive(false);

        StartCoroutine(AwardSequence());
    }

    private IEnumerator AwardSequence()
    {
        yield return new WaitForSeconds(1f);
        if (stageAnimator != null) stageAnimator.Play("CurtainsIntro");
        yield return new WaitForSeconds(5f);

        // Sort players
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

        // Reveal 4th Place
        if (pCount >= 4)
        {
            yield return StartCoroutine(RevealRoutine(sign4thPlace, fourthPlacePuppet, fourthPlaceID, pCount == 4));
        }

        // Reveal 3rd Place
        if (pCount >= 3)
        {
            yield return StartCoroutine(RevealRoutine(sign3rdPlace, thirdPlacePuppet, thirdPlaceID, pCount == 3));
        }

        // Reveal 2nd Place
        if (pCount >= 2)
        {
            yield return StartCoroutine(RevealRoutine(sign2ndPlace, secondPlacePuppet, secondPlaceID, pCount == 2));
        }

        // Reveal 1st Place
        yield return StartCoroutine(RevealRoutine(sign1stPlace, firstPlacePuppet, winnerID, false, true));
    }

    private IEnumerator RevealRoutine(Sprite signSprite, GameObject puppetObj, int playerID, bool isSadLoser, bool isWinner = false)
    {
        // 1. Swap the image to the correct placement and turn the sign ON (Plays Intro automatically)
        if (marqueeImage != null) marqueeImage.sprite = signSprite;
        if (marqueeAnimator != null) marqueeAnimator.gameObject.SetActive(true);

        // 2. Play Drumroll and wait in suspense (Sign is looping in Idle)
        if (drumrollAudio != null) drumrollAudio.Play();
        yield return new WaitForSeconds(drumrollWaitTime);

        // 3. THE REVEAL: Pop up the puppet!
        if (puppetObj != null)
        {
            puppetObj.SetActive(true);
            Image puppetImg = puppetObj.GetComponent<Image>();
            Animator puppetAnim = puppetObj.GetComponent<Animator>();

            // Assign proper color
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

            // Play correct animation and sound
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

        // Update the Win Text
        if (winText != null)
        {
            if (isWinner) winText.text = "1ST PLACE!";
            else if (isSadLoser) winText.text = "LAST PLACE...";
            else winText.text = puppetObj == secondPlacePuppet ? "2ND PLACE!" : puppetObj == thirdPlacePuppet ? "3RD PLACE!" : "4TH PLACE!";
            winText.gameObject.SetActive(true);
        }

        // 4. Bask in the glory/shame before putting the sign away
        yield return new WaitForSeconds(postRevealWaitTime);

        // 5. Trigger Outro on the sign
        if (marqueeAnimator != null)
        {
            marqueeAnimator.SetTrigger("PlayOutro");
            // Wait a second for the outro animation to physically slide off screen
            yield return new WaitForSeconds(1f);
            // Turn it off so it resets for the next player
            marqueeAnimator.gameObject.SetActive(false);
        }
    }
}