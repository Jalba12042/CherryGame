using UnityEngine;

public class BasketColorSync : MonoBehaviour
{
    public Renderer basketRenderer;

    [Header("Which material slot is the color overlay?")]
    public int colorMaterialIndex = 0;

    public Material[] colorMaterials;

    public void SetColor(int colorIndex)
    {
        if (basketRenderer == null) return;
        if (colorMaterials == null || colorMaterials.Length == 0) return;
        if (colorIndex < 0 || colorIndex >= colorMaterials.Length) return;

        // Copy current materials
        Material[] mats = basketRenderer.materials;

        // Replace ONLY the checkered/overlay material
        mats[colorMaterialIndex] = colorMaterials[colorIndex];

        // Apply back
        basketRenderer.materials = mats;
    }
}