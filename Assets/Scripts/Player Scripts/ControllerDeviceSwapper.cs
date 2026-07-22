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

    public void LockInDeviceIcons(Gamepad gamepad)
    {
        bool isPlaystation = gamepad is DualShockGamepad || gamepad.name.Contains("DualSense");

        if (isPlaystation)
        {
            if (confirmButtonImage != null) confirmButtonImage.sprite = psConfirm;
            if (backButtonImage != null) backButtonImage.sprite = psBack;

            // Loop through and just swap the sprite. No resizing!
            foreach (Image img in leftTriggerImages)
            {
                if (img != null) img.sprite = psL2;
            }

            foreach (Image img in rightTriggerImages)
            {
                if (img != null) img.sprite = psR2;
            }
        }
        else
        {
            if (confirmButtonImage != null) confirmButtonImage.sprite = xboxConfirm;
            if (backButtonImage != null) backButtonImage.sprite = xboxBack;

            foreach (Image img in leftTriggerImages)
            {
                if (img != null) img.sprite = xboxLT;
            }

            foreach (Image img in rightTriggerImages)
            {
                if (img != null) img.sprite = xboxRT;
            }
        }
    }
}