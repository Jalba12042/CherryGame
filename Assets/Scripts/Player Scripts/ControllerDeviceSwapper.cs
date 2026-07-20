using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

public class ControllerDeviceSwapper : MonoBehaviour
{
    [Header("The UI Images on Screen")]
    public Image confirmButtonImage; // The button for "Ready Up"
    public Image backButtonImage;    // The button for "Go Back"

    [Header("Your Xbox Art")]
    public Sprite xboxConfirm;       // The Green A
    public Sprite xboxBack;          // The Red B

    [Header("Your PlayStation Art")]
    public Sprite psConfirm;         // The Blue Cross (X)
    public Sprite psBack;            // The Red Circle (O)

    // Max's code will shout at this function the exact millisecond the player joins
    public void LockInDeviceIcons(Gamepad pad)
    {
        if (pad == null) return;

        // Unity checks the controller's internal hardware name
        if (pad is DualShockGamepad || pad.name.Contains("DualShock") || pad.name.Contains("DualSense"))
        {
            // It's a PlayStation controller! Swap to PS art.
            if (confirmButtonImage != null) confirmButtonImage.sprite = psConfirm;
            if (backButtonImage != null) backButtonImage.sprite = psBack;
        }
        else
        {
            // Default to Xbox art for everything else (Xbox, generic PC pads)
            if (confirmButtonImage != null) confirmButtonImage.sprite = xboxConfirm;
            if (backButtonImage != null) backButtonImage.sprite = xboxBack;
        }
    }
}
