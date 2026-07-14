using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class AwardRevealUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Animator envelopeAnimator;
    public TMP_Text categoryText;
    public TMP_Text winnerText;
    public Image momentPicture; // The icon that represents the award (Pistol, Cherry, etc.)

    [Header("Puppet Reveal")]
    public GameObject trophyPuppetGroup;
    public Image puppetImage;
    public Sprite[] puppetColors;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip drumrollSound;
    public AudioClip surpriseRevealSound;

    // Call this from GameWinScript
    public void StartReveal(string categoryName, Sprite momentSprite, int winnerID, string winnerName)
    {
        StartCoroutine(RevealSequence(categoryName, momentSprite, winnerID, winnerName));
    }

    private IEnumerator RevealSequence(string categoryName, Sprite momentSprite, int winnerID, string winnerName)
    {
        // 1. Setup the card
        categoryText.text = categoryName;
        momentPicture.sprite = momentSprite; // Set the icon
        winnerText.gameObject.SetActive(false); // Hide winner until the reveal
        trophyPuppetGroup.SetActive(false);

        // 2. Drumroll and Slide Up
        if (sfxSource != null && drumrollSound != null) sfxSource.PlayOneShot(drumrollSound);
        if (envelopeAnimator != null) envelopeAnimator.Play("CardSlideUp");

        yield return new WaitForSeconds(2.5f);

        // 3. THE REVEAL
        winnerText.text = winnerName + " WINS!";
        winnerText.gameObject.SetActive(true); // Show winner

        if (winnerID >= 0 && winnerID < puppetColors.Length)
            puppetImage.sprite = puppetColors[winnerID];

        trophyPuppetGroup.SetActive(true);
        if (sfxSource != null && surpriseRevealSound != null) sfxSource.PlayOneShot(surpriseRevealSound);

        yield return new WaitForSeconds(4f);

        // 4. Reset
        trophyPuppetGroup.SetActive(false);
        if (envelopeAnimator != null) envelopeAnimator.Play("CardSlideDown");

        yield return new WaitForSeconds(1f);
    }
}