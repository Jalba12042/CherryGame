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

    [Header("Scene To Load (Leave empty to use On Click list)")]
    public string sceneName;

    [Header("--- NEW: Transition Type ---")]
    [Tooltip("Check this box if this is the Online/Multiplayer button!")]
    public bool useMultiplayerTransition = false;

    [Header("Optional: play exit animation before loading")]
    [Tooltip("Drag the object holding your MenuExitOrchestrator here!")]
    public MenuExitOrchestrator exitOrchestrator;

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

        var btn = GetComponent<Button>();

        // SMART FIX: If SceneName is empty, but we have items in the OnClick list, run them instead!
        if (string.IsNullOrEmpty(sceneName) && btn != null && btn.onClick.GetPersistentEventCount() > 0)
        {
            locked = true;
            btn.interactable = false;
            btn.onClick.Invoke();
            return;
        }

        if (!isQuitButton && string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning($"[MenuSelectable] No scene assigned on {name}");
            return;
        }

        locked = true;

        if (btn) btn.interactable = false;

        if (isQuitButton)
        {
            StartCoroutine(QuitSequence());
            return;
        }

        if (exitOrchestrator != null)
        {
            // THE FIX: Route to the correct transition based on the checkbox!
            if (useMultiplayerTransition)
            {
                exitOrchestrator.ExitToMultiplayer(sceneName);
            }
            else
            {
                exitOrchestrator.ExitThenLoad(sceneName);
            }
        }
        else
        {
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