using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections; // NEW: Needed for Coroutines

[RequireComponent(typeof(Image))]
public class MenuSelectable : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite normalImage;
    public Sprite highlightImage;

    [Header("Action Settings")]
    public bool isQuitButton = false; // NEW: Check this box ONLY for the Quit sign!
    public float quitDelay = 0.5f;    // NEW: Gives the button click sound time to play before closing

    [Header("Scene To Load (Leave empty if Quit Button)")]
    public string sceneName;

    [Header("Optional: play exit animation before loading")]
    [Tooltip("If set, scene will load AFTER this orchestrator plays the exit animation.")]
    public MenuExitOrchestrator exitOrchestrator;

    private Image img;
    private bool locked; // prevents double-activations

    public RectTransform RectTransform { get; private set; }

    void Awake()
    {
        img = GetComponent<Image>();
        RectTransform = GetComponent<RectTransform>();
        if (!img) Debug.LogError($"MenuSelectable requires an Image on: {name}");
    }

    public void Highlight(bool on)
    {
        if (!img) return;
        img.sprite = (on && highlightImage) ? highlightImage : (normalImage ? normalImage : img.sprite);
    }

    public void Activate()
    {
        if (locked) return;

        // Check if it's missing a scene AND it's not a quit button
        if (!isQuitButton && string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning($"[MenuSelectable] No scene assigned on {name}");
            return;
        }

        locked = true;

        var btn = GetComponent<Button>();
        if (btn) btn.interactable = false; // avoid double presses

        // --- NEW: QUIT LOGIC ---
        if (isQuitButton)
        {
            StartCoroutine(QuitSequence());
            return;
        }

        // --- ORIGINAL SCENE LOAD LOGIC ---
        if (exitOrchestrator != null)
        {
            exitOrchestrator.ExitThenLoad(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private IEnumerator QuitSequence()
    {
        // Wait a tiny bit so your "Select" sound effect has time to play
        yield return new WaitForSeconds(quitDelay);

        Debug.Log("Quit Button Pressed! Shutting down the game...");

#if UNITY_EDITOR
        // This stops the game if you are testing inside the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // This fully closes the application when it is a built executable
            Application.Quit();
#endif
    }
}