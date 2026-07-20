using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class JoinIconFlasher : MonoBehaviour
{
    [Header("The Icons")]
    public Sprite xboxIcon;         // Drag your hand-drawn A button here
    public Sprite playstationIcon;  // Drag your hand-drawn X button here

    [Header("Timing")]
    public float flashInterval = 3f; // How many seconds before it swaps

    private Image buttonImage;

    void Awake()
    {
        // Grabs the Image component attached to this exact object
        buttonImage = GetComponent<Image>();
    }

    void OnEnable()
    {
        // Starts the flashing loop every time the Join panel appears
        StartCoroutine(FlashIconRoutine());
    }

    private IEnumerator FlashIconRoutine()
    {
        bool showXbox = true;

        while (true)
        {
            if (buttonImage != null && xboxIcon != null && playstationIcon != null)
            {
                // Swap the picture
                buttonImage.sprite = showXbox ? xboxIcon : playstationIcon;
            }

            // Flip the toggle for next time
            showXbox = !showXbox;

            // Wait 3 seconds
            yield return new WaitForSeconds(flashInterval);
        }
    }
}