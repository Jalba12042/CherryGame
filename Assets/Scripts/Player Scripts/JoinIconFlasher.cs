using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using System.Collections.Generic;

public class JoinIconFlasher : MonoBehaviour
{
    [Header("The Icons")]
    public Sprite xboxIcon;
    public Sprite playstationIcon;
    public Sprite keyboardIcon;

    [Header("Timing")]
    public float flashInterval = 3f;

    private Image imageComponent;
    private List<Sprite> activeIcons = new List<Sprite>();
    private float timer;
    private int currentIndex;

    void Start()
    {
        imageComponent = GetComponent<Image>();
        DetermineActiveIcons();
    }

    void Update()
    {
        // If we only have 1 icon type loaded (e.g. only Xbox controllers are plugged in),
        // there is no need to flash! It just stays on that one icon constantly.
        if (activeIcons.Count <= 1) return;

        timer += Time.deltaTime;
        if (timer >= flashInterval)
        {
            timer = 0f;
            currentIndex = (currentIndex + 1) % activeIcons.Count;
            if (imageComponent != null)
            {
                imageComponent.sprite = activeIcons[currentIndex];
            }
        }
    }

    public void DetermineActiveIcons()
    {
        activeIcons.Clear();
        bool hasXbox = false;
        bool hasPS = false;
        bool hasKeyboard = false;

        // Scan all connected hardware devices
        foreach (var device in InputSystem.devices)
        {
            if (device is Keyboard) hasKeyboard = true;
            else if (device is DualShockGamepad) hasPS = true; // Catches PlayStation 4/5 Controllers
            else if (device is Gamepad) hasXbox = true; // Catches Xbox and generic XInput PC Gamepads
        }

        // If for some reason nothing is detected, fallback to the Xbox icon
        if (!hasXbox && !hasPS && !hasKeyboard) hasXbox = true;

        // Add the relevant icons to our flashing list
        if (hasXbox && xboxIcon != null) activeIcons.Add(xboxIcon);
        if (hasPS && playstationIcon != null) activeIcons.Add(playstationIcon);

        // You can check GameManager here too if you only want the keyboard icon 
        // to show when a player explicitly picked keyboard in the lobby
        if (hasKeyboard && keyboardIcon != null) activeIcons.Add(keyboardIcon);

        // Set the starting icon immediately
        if (activeIcons.Count > 0 && imageComponent != null)
        {
            currentIndex = 0;
            imageComponent.sprite = activeIcons[0];
            timer = 0f;
        }
    }
}