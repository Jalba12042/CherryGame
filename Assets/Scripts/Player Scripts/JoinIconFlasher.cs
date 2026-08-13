using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class JoinIconFlasher : MonoBehaviour
{
    [Header("The Icons")]
    public Sprite xboxIcon;
    public Sprite playstationIcon;
    public Sprite keyboardIcon;

    [Header("Timing")]
    public float flashInterval = 3f;

    private Image buttonImage;

    void Awake()
    {
        buttonImage = GetComponent<Image>();
    }

    void OnEnable()
    {
        StartCoroutine(FlashIconRoutine());
    }

    private IEnumerator FlashIconRoutine()
    {
        int iconState = 0;

        while (true)
        {
            // --- THE FIX: Check if anyone is already using the keyboard ---
            bool isKeyboardTaken = false;
            if (InputManager.Instance != null)
            {
                for (int i = 1; i <= 4; i++)
                {
                    if (InputManager.Instance.IsKeyboardPlayer(i))
                    {
                        isKeyboardTaken = true;
                        break;
                    }
                }
            }

            // If it's the keyboard's turn to flash (2), but it's already taken, skip straight back to Xbox (0)!
            if (iconState == 2 && isKeyboardTaken)
            {
                iconState = 0;
            }
            // --------------------------------------------------------------

            if (buttonImage != null)
            {
                if (iconState == 0 && xboxIcon != null)
                {
                    buttonImage.sprite = xboxIcon;
                }
                else if (iconState == 1 && playstationIcon != null)
                {
                    buttonImage.sprite = playstationIcon;
                }
                else if (iconState == 2 && keyboardIcon != null)
                {
                    buttonImage.sprite = keyboardIcon;
                }
            }

            // Wait 3 seconds
            yield return new WaitForSeconds(flashInterval);

            // Move to the next icon for the next loop
            iconState++;

            // If we've gone past the keyboard (2), reset back to Xbox (0)
            if (iconState > 2)
            {
                iconState = 0;
            }
        }
    }
}