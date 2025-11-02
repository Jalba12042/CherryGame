using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuSelectable : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite normalImage;
    public Sprite highlightImage;

    [Header("Scene To Load")]
    public string sceneName;

    private Image img;

    void Awake()
    {
        img = GetComponent<Image>();
        if (img == null)
            Debug.LogError("MenuSelectable requires an Image component on: " + gameObject.name);
    }

    public void Highlight(bool on)
    {
        if (img == null) return;

        if (on && highlightImage != null)
            img.sprite = highlightImage;
        else if (!on && normalImage != null)
            img.sprite = normalImage;
    }

    public void Activate()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("No scene assigned on: " + gameObject.name);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}

