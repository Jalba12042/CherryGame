using UnityEngine;
using TMPro;

public class TimerUIManager : MonoBehaviour
{
    [Header("UI Elements to Control")]
    public GameObject timerBackgroundObject;
    public Animator timerAnimator;
    public TMP_Text timerText;

    [Header("Warning Signs")]
    public GameObject sign30Sec;
    public GameObject sign10Sec;

    // These memory flags stop the game from turning the signs on 60 times a second
    private bool hasShown30 = false;
    private bool hasShown10 = false;

    void Start()
    {
        // Force them OFF at the start of the level!
        if (sign30Sec != null) sign30Sec.SetActive(false);
        if (sign10Sec != null) sign10Sec.SetActive(false);

        // Reset our memory flags every time the scene starts
        hasShown30 = false;
        hasShown10 = false;
    }

    void Update()
    {
        // Make sure the RoundManager exists and the round is actually playing
        if (RoundManager.Instance != null && RoundManager.Instance.currRoundActive)
        {
            // Calculate exactly how much time is left in the round
            float remainingTime = RoundManager.Instance.currRoundDurationInSecs - RoundManager.Instance.currRoundProgress;

            // --- 30 SECOND CHECK ---
            if (remainingTime <= 30f && remainingTime > 10f && !hasShown30)
            {
                if (sign30Sec != null) sign30Sec.SetActive(true);

                Animator anim30 = sign30Sec.GetComponent<Animator>();
                if (anim30 != null) anim30.SetTrigger("Show");

                hasShown30 = true;
            }

            // --- 10 SECOND CHECK ---
            if (remainingTime <= 10f && remainingTime > 0f && !hasShown10)
            {
                if (sign30Sec != null) sign30Sec.SetActive(false);

                if (sign10Sec != null) sign10Sec.SetActive(true);

                Animator anim10 = sign10Sec.GetComponent<Animator>();
                if (anim10 != null) anim10.SetTrigger("Show");

                hasShown10 = true;
            }
        }
    }

    // --- NEW: The missing method that FakeLoadingScreen is looking for! ---
    public void RevealTimer()
    {
        // Turns on the cardboard timer background
        if (timerBackgroundObject != null)
        {
            timerBackgroundObject.SetActive(true);
        }

        // Triggers the animation if you have one attached
        if (timerAnimator != null)
        {
            timerAnimator.SetTrigger("StartTimer");
        }
    }
}