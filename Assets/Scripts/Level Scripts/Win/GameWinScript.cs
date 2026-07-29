using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameWinScript : MonoBehaviour
{
    [Header("UI & Stage")]
    public Animator stageAnimator;
    public TMP_Text firstPlaceText;

    [Header("Puppet Setup")]
    [Tooltip("Drag the hidden puppet GameObject from your Canvas here")]
    public GameObject firstPlacePuppet;
    public Sprite[] colorSprites;

    [Header("Audio Polish")]
    public AudioSource drumrollAudio;
    [Tooltip("How long the drumroll plays before the puppet pops up")]
    public float drumrollWaitTime = 2f;

    void Start()
    {
        // Hide text at start
        if (firstPlaceText != null) firstPlaceText.gameObject.SetActive(false);

        // Ensure the puppet starts completely invisible
        if (firstPlacePuppet != null) firstPlacePuppet.SetActive(false);

        StartCoroutine(SimpleWinSequence());
    }

    private IEnumerator SimpleWinSequence()
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

        // 5. Find the winner in GameManager
        int winnerID = 0;
        int highestScore = -1;

        if (GameManager.Instance != null)
        {
            for (int i = 0; i < GameManager.Instance.playerCount; i++)
            {
                if (GameManager.Instance.playerTotalScores[i] > highestScore)
                {
                    highestScore = GameManager.Instance.playerTotalScores[i];
                    winnerID = i;
                }
            }
        }

        // 6. Reveal and Setup the existing Puppet
        if (firstPlacePuppet != null)
        {
            firstPlacePuppet.SetActive(true);

            Image puppetImg = firstPlacePuppet.GetComponent<Image>();
            if (puppetImg != null && GameManager.Instance != null)
            {
                int colorIndex = GameManager.Instance.playerCustomizations[winnerID].colorIndex;
                if (colorIndex < colorSprites.Length)
                {
                    puppetImg.sprite = colorSprites[colorIndex];
                }
            }

            Animator anim = firstPlacePuppet.GetComponent<Animator>();
            if (anim != null) anim.Play("PuppetSlideUp_1stPlace");
        }

        // 7. Flash Text
        if (firstPlaceText != null)
        {
            firstPlaceText.text = "1ST PLACE!";
            firstPlaceText.gameObject.SetActive(true);
        }
    }
}