using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ImageButtonSwitch : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Image References")]
    public Sprite normalImage;
    public Sprite highlightImage;

    [Header("Scene To Load")]
    public string sceneName = "LocalScene"; // Change to your local scene name

    private Image buttonImage;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        if (buttonImage != null && normalImage != null)
            buttonImage.sprite = normalImage;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Highlight(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Highlight(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Activate();
    }

    // --- Added for controller support ---
    public void Highlight(bool active)
    {
        if (buttonImage != null)
            buttonImage.sprite = active ? highlightImage : normalImage;
    }

    public void Activate()
    {
        SceneManager.LoadScene(sceneName);
    }
}
