using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Image))]
public class MenuSelectable : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite normalImage;
    public Sprite highlightImage;

    [Header("Action Settings")]
    public bool isQuitButton = false;
    public float quitDelay = 0.5f;

    [Header("Scene To Load (Leave empty if Quit Button)")]
    public string sceneName;

    [Header("Optional: play exit animation before loading")]
    [Tooltip("Drag the object holding your MenuExitOrchestrator here!")]
    public MenuExitOrchestrator exitOrchestrator; // <-- This is the key!

    private Image img;
    private bool locked;

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

        if (!isQuitButton && string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning($"[MenuSelectable] No scene assigned on {name}");
            return;
        }

        locked = true;

        var btn = GetComponent<Button>();
        if (btn) btn.interactable = false;

        if (isQuitButton)
        {
            StartCoroutine(QuitSequence());
            return;
        }

        // Trigger the Orchestrator (which now handles the menu sliding AND the paper wipe)
        if (exitOrchestrator != null)
        {
            exitOrchestrator.ExitThenLoad(sceneName);
        }
        else
        {
            // Failsafe
            SceneManager.LoadScene(sceneName);
        }
    }

    private IEnumerator QuitSequence()
    {
        yield return new WaitForSeconds(quitDelay);
        Debug.Log("Quit Button Pressed! Shutting down the game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}