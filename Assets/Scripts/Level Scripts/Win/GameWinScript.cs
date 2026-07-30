using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameWinScript : MonoBehaviour
{
    [Header("UI & Stage")]
    public Animator stageAnimator;
    [Tooltip("Drag your WinTxt here")]
    public TMP_Text winText;

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
    [Tooltip("Sprites to use when a player gets Last Place")]
    public Sprite[] colorSpritesLastPlace;

    [Header("Audio Polish")]
    public AudioSource drumrollAudio;
    [Tooltip("How long the drumroll plays before the first reveal pops up")]
    public float drumrollWaitTime = 5f;
    [Tooltip("How many seconds to wait between each puppet popping up")]
    public float timeBetweenReveals = 4f;

    void Start()
    {
        // Hide UI and all puppets at the start
        if (winText != null) winText.gameObject.SetActive(false);
        if (firstPlacePuppet != null) firstPlacePuppet.SetActive(false);
        if (secondPlacePuppet != null) secondPlacePuppet.SetActive(false);
        if (thirdPlacePuppet != null) thirdPlacePuppet.SetActive(false);
        if (fourthPlacePuppet != null) fourthPlacePuppet.SetActive(false);

        StartCoroutine(AwardSequence());
    }

    private IEnumerator AwardSequence()
    {
        // 1. Wait a second, then trigger the curtain animation
        yield return new WaitForSeconds(1f);
        if (stageAnimator != null) stageAnimator.Play("CurtainsIntro");

        // 2. Wait exactly 5 seconds for the curtains to finish opening
        yield return new WaitForSeconds(5f);

        // 3. Play the drumroll sound
        if (drumrollAudio != null) drumrollAudio.Play();

        // 4. Wait for the drumroll to build up suspense
        yield return new WaitForSeconds(drumrollWaitTime);

        // 5. Sort the scores to find 1st, 2nd, 3rd, and 4th place
        List<int> sortedPlayers = new List<int>();
        int pCount = 0;

        if (GameManager.Instance != null)
        {
            pCount = GameManager.Instance.playerCount;
            for (int i = 0; i < pCount; i++)
            {
                sortedPlayers.Add(i);
            }

            // Sorts players highest to lowest based on total score
            sortedPlayers.Sort((p1, p2) => GameManager.Instance.playerTotalScores[p2].CompareTo(GameManager.Instance.playerTotalScores[p1]));
        }

        // Safely grab the IDs (defaulting to 0 if something is empty)
        int winnerID = sortedPlayers.Count > 0 ? sortedPlayers[0] : 0;
        int secondPlaceID = sortedPlayers.Count > 1 ? sortedPlayers[1] : 0;
        int thirdPlaceID = sortedPlayers.Count > 2 ? sortedPlayers[2] : 0;
        int fourthPlaceID = sortedPlayers.Count > 3 ? sortedPlayers[3] : 0;

        // --- 6. Reveal 4th Place Puppet (If there are 4 players) ---
        if (pCount >= 4 && fourthPlacePuppet != null)
        {
            fourthPlacePuppet.SetActive(true);
            Image puppetImg4th = fourthPlacePuppet.GetComponent<Image>();
            Animator anim4th = fourthPlacePuppet.GetComponent<Animator>();

            if (pCount == 4) // Last place in a 4-player game
            {
                if (puppetImg4th != null && GameManager.Instance != null)
                {
                    int colorIndex = GameManager.Instance.playerCustomizations[fourthPlaceID].colorIndex;
                    if (colorIndex < colorSpritesLastPlace.Length) puppetImg4th.sprite = colorSpritesLastPlace[colorIndex];
                }
                if (anim4th != null) anim4th.Play("PuppetSad_4thPlace");
                if (winText != null) { winText.text = "LAST PLACE..."; winText.gameObject.SetActive(true); }
            }
            else
            {
                if (puppetImg4th != null && GameManager.Instance != null)
                {
                    int colorIndex = GameManager.Instance.playerCustomizations[fourthPlaceID].colorIndex;
                    if (colorIndex < colorSprites4thPlace.Length) puppetImg4th.sprite = colorSprites4thPlace[colorIndex];
                }
                if (anim4th != null) anim4th.Play("PuppetSlideUp_4thPlace");
                if (winText != null) { winText.text = "4TH PLACE!"; winText.gameObject.SetActive(true); }
            }

            yield return new WaitForSeconds(timeBetweenReveals);
        }

        // --- 7. Reveal 3rd Place Puppet (If there are at least 3 players) ---
        if (pCount >= 3 && thirdPlacePuppet != null)
        {
            thirdPlacePuppet.SetActive(true);
            Image puppetImg3rd = thirdPlacePuppet.GetComponent<Image>();
            Animator anim3rd = thirdPlacePuppet.GetComponent<Animator>();

            if (pCount == 3) // Last place in a 3-player game
            {
                if (puppetImg3rd != null && GameManager.Instance != null)
                {
                    int colorIndex = GameManager.Instance.playerCustomizations[thirdPlaceID].colorIndex;
                    if (colorIndex < colorSpritesLastPlace.Length) puppetImg3rd.sprite = colorSpritesLastPlace[colorIndex];
                }
                if (anim3rd != null) anim3rd.Play("PuppetSad_3rdPlace");
                if (winText != null) { winText.text = "LAST PLACE..."; winText.gameObject.SetActive(true); }
            }
            else // 3rd place in a 4-player game
            {
                if (puppetImg3rd != null && GameManager.Instance != null)
                {
                    int colorIndex = GameManager.Instance.playerCustomizations[thirdPlaceID].colorIndex;
                    if (colorIndex < colorSprites3rdPlace.Length) puppetImg3rd.sprite = colorSprites3rdPlace[colorIndex];
                }
                if (anim3rd != null) anim3rd.Play("PuppetSlideUp_3rdPlace");
                if (winText != null) { winText.text = "3RD PLACE!"; winText.gameObject.SetActive(true); }
            }

            yield return new WaitForSeconds(timeBetweenReveals);
        }

        // --- 8. Reveal 2nd Place Puppet (If there are at least 2 players) ---
        if (pCount >= 2 && secondPlacePuppet != null)
        {
            secondPlacePuppet.SetActive(true);
            Image puppetImg2nd = secondPlacePuppet.GetComponent<Image>();
            Animator anim2nd = secondPlacePuppet.GetComponent<Animator>();

            if (pCount == 2) // Last place in a 2-player game
            {
                if (puppetImg2nd != null && GameManager.Instance != null)
                {
                    int colorIndex = GameManager.Instance.playerCustomizations[secondPlaceID].colorIndex;
                    if (colorIndex < colorSpritesLastPlace.Length) puppetImg2nd.sprite = colorSpritesLastPlace[colorIndex];
                }
                if (anim2nd != null) anim2nd.Play("PuppetSad_2ndPlace");
                if (winText != null) { winText.text = "LAST PLACE..."; winText.gameObject.SetActive(true); }
            }
            else // 2nd place in a 3 or 4-player game
            {
                if (puppetImg2nd != null && GameManager.Instance != null)
                {
                    int colorIndex = GameManager.Instance.playerCustomizations[secondPlaceID].colorIndex;
                    if (colorIndex < colorSprites2ndPlace.Length) puppetImg2nd.sprite = colorSprites2ndPlace[colorIndex];
                }
                if (anim2nd != null) anim2nd.Play("PuppetSlideUp_2ndPlace");
                if (winText != null) { winText.text = "2ND PLACE!"; winText.gameObject.SetActive(true); }
            }

            yield return new WaitForSeconds(timeBetweenReveals);
        }

        // --- 9. Reveal 1st Place Puppet (Always gets the trophy!) ---
        if (firstPlacePuppet != null)
        {
            firstPlacePuppet.SetActive(true);
            Image puppetImg1st = firstPlacePuppet.GetComponent<Image>();

            if (puppetImg1st != null && GameManager.Instance != null)
            {
                int colorIndex = GameManager.Instance.playerCustomizations[winnerID].colorIndex;
                if (colorIndex < colorSprites1stPlace.Length) puppetImg1st.sprite = colorSprites1stPlace[colorIndex];
            }

            Animator anim1st = firstPlacePuppet.GetComponent<Animator>();
            if (anim1st != null) anim1st.Play("PuppetSlideUp_1stPlace");
        }

        // 10. Update text to 1st Place
        if (winText != null)
        {
            winText.text = "1ST PLACE!";
            winText.gameObject.SetActive(true);
        }
    }
}