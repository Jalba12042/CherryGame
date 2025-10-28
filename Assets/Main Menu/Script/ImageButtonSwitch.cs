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
        if (buttonImage != null && highlightImage != null)
            buttonImage.sprite = highlightImage;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonImage != null && normalImage != null)
            buttonImage.sprite = normalImage;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SceneManager.LoadScene(sceneName);
    }
}
