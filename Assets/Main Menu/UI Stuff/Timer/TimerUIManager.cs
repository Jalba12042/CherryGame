using UnityEngine;

public class TimerUIManager : MonoBehaviour
{
    [Header("UI Elements to Control")]
    public GameObject timerBackgroundObject; // The whole object
    public Animator timerAnimator;           // The animator on that object

    private void Start()
    {
        // 1. Force the timer background to be completely OFF when the scene loads
        if (timerBackgroundObject != null)
        {
            timerBackgroundObject.SetActive(false);
        }
    }

    // 2. Your Start Round button will trigger this function
    public void RevealTimer()
    {
        if (timerBackgroundObject != null)
        {
            // Turn the whole object ON
            timerBackgroundObject.SetActive(true);

            // Play the flipbook animation
            if (timerAnimator != null)
            {
                timerAnimator.SetTrigger("StartTimer");
            }
        }
    }
}
