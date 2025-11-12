using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Image))]
public class MenuSelectable : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite normalImage;
    public Sprite highlightImage;

    [Header("Scene To Load")]
    public string sceneName;

    [Header("Optional: play exit animation before loading")]
    [Tooltip("If set, scene will load AFTER this orchestrator plays the exit animation.")]
    public MenuExitOrchestrator exitOrchestrator;

    private Image img;
    private bool locked; // prevents double-activations

    void Awake()
    {
        img = GetComponent<Image>();
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

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning($"[MenuSelectable] No scene assigned on {name}");
            return;
        }

        locked = true;

        // If an orchestrator is assigned, let it play the EXIT animations, then load.
        if (exitOrchestrator != null)
        {
            var btn = GetComponent<Button>();
            if (btn) btn.interactable = false; // avoid double presses while exiting
            exitOrchestrator.ExitThenLoad(sceneName);
        }
        else
        {
            // Fallback: immediate load (old behavior)
            SceneManager.LoadScene(sceneName);
        }
    }
}
