using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

public class ControllerDeviceSwapper : MonoBehaviour
{
    [Header("The UI Images on Screen")]
    public Image confirmButtonImage;
    public Image backButtonImage;
    public Image[] leftTriggerImages;
    public Image[] rightTriggerImages;

    [Header("Your Xbox Art")]
    public Sprite xboxConfirm;
    public Sprite xboxBack;
    public Sprite xboxLT;
    public Sprite xboxRT;

    [Header("Your PlayStation Art")]
    public Sprite psConfirm;
    public Sprite psBack;
    public Sprite psL2;
    public Sprite psR2;

    [Header("Your Keyboard Art")]
    public Sprite kbConfirm; // Drop your Spacebar art here
    public Sprite kbBack;    // Drop your 'E' key art here
    public Sprite kbLeft;    // Drop your 'A' or 'Left Arrow' art here
    public Sprite kbRight;   // Drop your 'D' or 'Right Arrow' art here

    // We changed this to 'InputDevice' so it can accept Keyboards OR Gamepads!
    public void LockInDeviceIcons(InputDevice device)
    {
        // 1. Check if the device is a Keyboard
        if (device is Keyboard)
        {
            if (confirmButtonImage != null) confirmButtonImage.sprite = kbConfirm;
            if (backButtonImage != null) backButtonImage.sprite = kbBack;

            foreach (Image img in leftTriggerImages) { if (img != null) img.sprite = kbLeft; }
            foreach (Image img in rightTriggerImages) { if (img != null) img.sprite = kbRight; }

            return; // Stop running the rest of the code since we already set the keyboard UI
        }

        // 2. If it's not a Keyboard, check if it's a PlayStation controller
        bool isPlaystation = device is DualShockGamepad || device.name.Contains("DualSense");

        if (isPlaystation)
        {
            if (confirmButtonImage != null) confirmButtonImage.sprite = psConfirm;
            if (backButtonImage != null) backButtonImage.sprite = psBack;

            foreach (Image img in leftTriggerImages) { if (img != null) img.sprite = psL2; }
            foreach (Image img in rightTriggerImages) { if (img != null) img.sprite = psR2; }
        }
        // 3. Otherwise, default to Xbox
        else
        {
            if (confirmButtonImage != null) confirmButtonImage.sprite = xboxConfirm;
            if (backButtonImage != null) backButtonImage.sprite = xboxBack;

            foreach (Image img in leftTriggerImages) { if (img != null) img.sprite = xboxLT; }
            foreach (Image img in rightTriggerImages) { if (img != null) img.sprite = xboxRT; }
        }
    }
}