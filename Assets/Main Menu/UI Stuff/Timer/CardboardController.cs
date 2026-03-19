using UnityEngine;
using UnityEngine.UI; // Required to control the UI Image component

public class CardboardController : MonoBehaviour
{
    private Animator animator;
    private Image cardboardImage;

    void Awake()
    {
        // Find the Animator and Image components on this GameObject
        animator = GetComponent<Animator>();
        cardboardImage = GetComponent<Image>();

        // Turn off the image component immediately so it is invisible when the game loads
        if (cardboardImage != null)
        {
            cardboardImage.enabled = false;
        }
    }

    // This is the function you linked to your StartRoundButton
    public void PlayIntroAndStartTimer()
    {
        // 1. Turn the image back on so we can see the flipbook animation
        if (cardboardImage != null)
        {
            cardboardImage.enabled = true;
        }

        // 2. Trigger the Animator to transition from 'Hidden' to 'Cardboard_Intro'
        if (animator != null)
        {
            animator.SetTrigger("StartTimer");
        }
    }
}
