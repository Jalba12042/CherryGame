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

    void Start()
    {
        imageComponent = GetComponent<Image>();
        DetermineActiveIcons();
    }

    void Update()
    {
        // If we only have 1 icon type loaded, no need to flash!
        if (activeIcons.Count <= 1) return;

        // THE FIX: Use the global game clock so all flashers in the scene are perfectly synced!
        int currentIndex = Mathf.FloorToInt(Time.time / flashInterval) % activeIcons.Count;

        if (imageComponent != null)
        {
            imageComponent.sprite = activeIcons[currentIndex];
        }
    }

    public void DetermineActiveIcons()
    {
        activeIcons.Clear();
        bool hasXbox = false;
        bool hasPS = false;
        bool hasKeyboard = false;

        foreach (var device in InputSystem.devices)
        {
            if (device is Keyboard) hasKeyboard = true;
            else if (device is DualShockGamepad) hasPS = true;
            else if (device is Gamepad) hasXbox = true;
        }

        if (!hasXbox && !hasPS && !hasKeyboard) hasXbox = true;

        if (hasXbox && xboxIcon != null) activeIcons.Add(xboxIcon);
        if (hasPS && playstationIcon != null) activeIcons.Add(playstationIcon);
        if (hasKeyboard && keyboardIcon != null) activeIcons.Add(keyboardIcon);

        if (activeIcons.Count > 0 && imageComponent != null)
        {
            imageComponent.sprite = activeIcons[0];
        }
    }
}