using UnityEngine;

public class PlayerCustomizationUI : MonoBehaviour
{
    [System.Serializable]
    public class Category
    {
        public GameObject root;       // The whole category object
        public GameObject leftArrow;
        public GameObject rightArrow;

        public GameObject LTImage; // child under leftArrow
        public GameObject RTImage; // child under rightArrow
    }

    public Category[] categories;

    private int currentIndex = 0;

    [Header("Color Customization")]
    public Material[] colorMaterials;

    private int currentColorIndex = 0;


    void Start()
    {
        UpdateCategoryHighlight();
    }

    public void Initialize()
    {
        currentIndex = 0;
        UpdateCategoryHighlight();
    }


    public void MoveSelection(int direction)
    {
        currentIndex += direction;

        if (currentIndex < 0) currentIndex = categories.Length - 1;
        if (currentIndex >= categories.Length) currentIndex = 0;

        UpdateCategoryHighlight();
    }

    private void UpdateCategoryHighlight()
    {
        for (int i = 0; i < categories.Length; i++)
        {
            bool active = (i == currentIndex);

            // Arrows ALWAYS stay on
            categories[i].leftArrow.SetActive(true);
            categories[i].rightArrow.SetActive(true);

            // ONLY toggle RT / LT images
            if (categories[i].LTImage != null)
                categories[i].LTImage.SetActive(active);

            if (categories[i].RTImage != null)
                categories[i].RTImage.SetActive(active);
        }
    }

    public int GetCurrentCategoryIndex()
    {
        return currentIndex;
    }

    public void ChangeColor(int direction, GameObject playerModel, int playerIndex)
    {
        if (colorMaterials.Length == 0 || playerModel == null)
            return;

        PlayerJoinController joinController = FindFirstObjectByType<PlayerJoinController>();

        int nextIndex = currentColorIndex;

        for (int i = 0; i < colorMaterials.Length; i++)
        {
            nextIndex += direction;

            if (nextIndex < 0)
                nextIndex = colorMaterials.Length - 1;

            if (nextIndex >= colorMaterials.Length)
                nextIndex = 0;

            // Skip taken colors
            if (!joinController.IsColorTaken(nextIndex, playerIndex))
            {
                currentColorIndex = nextIndex;
                ApplyColor(playerModel);
                return;
            }
        }

        // If ALL colors are taken, do nothing
    }

    void ApplyColor(GameObject playerModel)
    {
        Renderer renderer = playerModel.GetComponentInChildren<Renderer>();

        if (renderer != null)
        {
            renderer.material = colorMaterials[currentColorIndex];
        }
    }

    public void SetColorIndex(int index, GameObject playerModel)
    {
        if (colorMaterials.Length == 0)
            return;

        currentColorIndex = Mathf.Clamp(index, 0, colorMaterials.Length - 1);
        ApplyColor(playerModel);
    }

    public int GetCurrentColorIndex()
    {
        return currentColorIndex;
    }
}
