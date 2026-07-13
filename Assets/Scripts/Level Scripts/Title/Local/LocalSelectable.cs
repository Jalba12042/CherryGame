using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LocalSelectable : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite normalImage;
    public Sprite highlightImage;

    [Header("Scene To Load (optional)")]
    public string sceneName;

    private Image img;

    public RectTransform RectTransform { get; private set; }

    void Awake()
    {
        img = GetComponent<Image>();
        RectTransform = GetComponent<RectTransform>();
        if (img == null)
            Debug.LogError("LocalSelectable needs an Image component on " + gameObject.name);
    }

    public void Highlight(bool on)
    {
        if (img == null) return;

        if (on && highlightImage != null)
            img.sprite = highlightImage;
        else if (!on && normalImage != null)
            img.sprite = normalImage;
    }

    // This allows clicking with a cursor too
    public void Activate()
    {
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.Log("LocalSelectable Activate() called � no sceneName set, but that's fine for LocalMenuController");
    }
}

