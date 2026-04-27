using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerAutoSwitch : MonoBehaviour
{
    [Header("Controller Images")]
    public GameObject xboxLayout;
    public GameObject playstationLayout;

    private string lastDetectedController = "";

    void Start()
    {
        // Default to Xbox layout right when the scene opens
        if (xboxLayout != null) xboxLayout.SetActive(true);
        if (playstationLayout != null) playstationLayout.SetActive(false);
    }

    void Update()
    {
        // If no gamepad is plugged in, just do nothing
        if (Gamepad.current == null) return;

        // Get the name of the controller (and make it lowercase to check it easily)
        string currentDeviceName = Gamepad.current.name.ToLower();

        // Only swap the images if the controller actually changed
        if (currentDeviceName != lastDetectedController)
        {
            lastDetectedController = currentDeviceName;

            // Check if the name has PlayStation keywords in it
            if (currentDeviceName.Contains("dualshock") || currentDeviceName.Contains("dualsense") || currentDeviceName.Contains("playstation"))
            {
                playstationLayout.SetActive(true);
                xboxLayout.SetActive(false);
                Debug.Log("🎮 PlayStation Controller Detected! Showing PS layout.");
            }
            else
            {
                // If it's an Xbox or generic PC controller, default to the Xbox layout
                playstationLayout.SetActive(false);
                xboxLayout.SetActive(true);
                Debug.Log("🎮 Xbox Controller Detected! Showing Xbox layout.");
            }
        }
    }
}