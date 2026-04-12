using UnityEngine;

public class MeteorShadowExpand : MonoBehaviour
{
    public Transform fallingObject;

    [Header("Distance Settings")]
    public float maxDistance = 20f;
    public float minScale = 20f;
    public float maxScale = 50f;

    [Header("Color Settings")]
    public Color farColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    public Color closeColor = new Color(0f, 0f, 0f, 1f);

    [Header("Fade Settings")]
    public float fadeSpeed = 5f;
    public float impactThreshold = 0.2f;

    private Renderer rend;
    private Material mat;
    private bool fadingOut = false;

    void Start()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material;
    }

    void Update()
    {
        if (fadingOut)
        {
            FadeOut();
            return;
        }

        RaycastHit hit;

        // Cast downward in standard Unity Y-up
        if (Physics.Raycast(fallingObject.position, Vector3.down, out hit, maxDistance))
        {
            float distance = hit.distance;

            // 0 = far away, 1 = close to ground
            float t = 1f - Mathf.Clamp01(distance / maxDistance);

            // Shadow gets bigger as object gets closer
            float scale = Mathf.Lerp(minScale, maxScale, t);
            transform.localScale = new Vector3(scale, scale, scale);

            // Shadow gets darker as object gets closer
            mat.color = Color.Lerp(farColor, closeColor, t);

            // Begin fading when almost touching ground
            if (distance <= impactThreshold)
            {
                fadingOut = true;
            }
        }
    }

    void FadeOut()
    {
        Color currentColor = mat.color;
        currentColor.a = Mathf.Lerp(currentColor.a, 0f, Time.deltaTime * fadeSpeed);
        mat.color = currentColor;

        if (currentColor.a <= 0.01f)
        {
            gameObject.SetActive(false);
        }
    }
}