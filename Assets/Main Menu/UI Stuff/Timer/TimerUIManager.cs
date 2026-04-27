using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem; // Needed to disable the controllers!

public class TimerUIManager : MonoBehaviour
{
    [Header("UI Elements to Control")]
    public GameObject timerBackgroundObject;
    public Animator timerAnimator;
    public TMP_Text timerText;               // NEW: Drag your Timer Text object here!

    [Header("Warning Signs")]
    public GameObject sign30Sec; // NEW: Drag your 30Sec object here!
    public GameObject sign10Sec; // NEW: Drag your 10Sec object here!

    private bool shown30 = false;
    private bool shown10 = false;
    private bool roundOver = false;
    private bool isTimerActive = false;

    private void Start()
    {
        // 1. Reset time speed (Just in case the last round ended in slow-mo!)
        Time.timeScale = 1f;

        // 2. Hide everything at the start
        if (timerBackgroundObject != null) timerBackgroundObject.SetActive(false);
        if (sign30Sec != null) sign30Sec.SetActive(false);
        if (sign10Sec != null) sign10Sec.SetActive(false);
    }

    public void RevealTimer()
    {
        if (timerBackgroundObject != null)
        {
            timerBackgroundObject.SetActive(true);
            if (timerAnimator != null) timerAnimator.SetTrigger("StartTimer");
        }

        // Turn on the clock!
        isTimerActive = true;
    }

    private void Update()
    {
        // Only run if the timer has been revealed and the round isn't over yet
        if (!isTimerActive || roundOver || RoundManager.Instance == null) return;

        float progress = RoundManager.Instance.currRoundProgress;

        // --- THE FIX IS RIGHT HERE: Add (int) ---
        int duration = (int)RoundManager.Instance.currRoundDurationInSecs;

        int remaining = duration - (int)progress;

        // --- 1. SHOW "GO!" AND THE CLOCK ---
        if (progress < 1.5f) // Shows "GO!" for the first 1.5 seconds of the round
        {
            if (timerText != null) timerText.text = "GO!";
        }
        else if (remaining > 0)
        {
            if (timerText != null) timerText.text = remaining.ToString();
        }

        // --- 2. THE 30 SECOND WARNING ---
        if (remaining == 30 && !shown30)
        {
            shown30 = true;
            if (sign30Sec != null) sign30Sec.SetActive(true);

            // Automatically hides the sign after 3 seconds
            StartCoroutine(HideSignAfterTime(sign30Sec, 3f));
        }

        // --- 3. THE 10 SECOND WARNING ---
        if (remaining == 10 && !shown10)
        {
            shown10 = true;
            if (sign10Sec != null) sign10Sec.SetActive(true);

            // Automatically hides the sign after 3 seconds
            StartCoroutine(HideSignAfterTime(sign10Sec, 3f));
        }

        // --- 4. TIME'S UP! (DRAMATIC FINISH) ---
        if (remaining <= 0)
        {
            roundOver = true;
            if (timerText != null) timerText.text = "TIME'S UP!";
            TriggerRoundEndEffect();
        }
    }

    private IEnumerator HideSignAfterTime(GameObject sign, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (sign != null) sign.SetActive(false);
    }

    private void TriggerRoundEndEffect()
    {
        // 1. Slow down time for a dramatic finish!
        Time.timeScale = 0.3f;

        // 2. Find every player controller in the scene and shut off their inputs
        PlayerInput[] allPlayers = FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
        foreach (PlayerInput player in allPlayers)
        {
            player.enabled = false;
        }
    }
}
