using UnityEngine;

public class MeteorShadowExpand : MonoBehaviour
{
    public Transform fallingObject;
    public LayerMask groundLayer;

    [Header("Distance Settings")]
    public float maxDistance = 20f;
    public float minScale = 20f;
    public float maxScale = 50f;

    [Header("Color Settings")]
    public Color farColor = Color.gray;
    public Color closeColor = Color.black;

    [Header("Fade Settings")]
    public float fadeSpeed = 5f;
    public float impactThreshold = 0.5f;

    [Header("Impact Effect")]
    public SpawnCrack crackSpawner;

    private Renderer rend;
    private Material mat;
    private bool fadingOut = false;
    private bool impactTriggered = false;


    void Start()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material;

        transform.parent = null;
    }

    void Update()
    {
        if (fallingObject == null)
        {
            Destroy(gameObject);
            return;
        }

        if (fadingOut)
        {
            FadeOut();
            return;
        }

        RaycastHit hit;

        // IMPORTANT: raycast downward
        if (Physics.Raycast(fallingObject.position, Vector3.down, out hit, maxDistance, groundLayer))
        {
            float distance = hit.distance;

            float t = 1f - Mathf.Clamp01(distance / maxDistance);

            // SCALE SHADOW
            float scale = Mathf.Lerp(minScale, maxScale, t);
            transform.localScale = new Vector3(scale, scale, scale);

            // DARKEN SHADOW
            mat.color = Color.Lerp(farColor, closeColor, t);

            // IMPACT
            if (distance <= impactThreshold)
            {
                fadingOut = true;
            }
        }
    }

    void FadeOut()
    {
        Color currentColor = mat.color;
        currentColor.a -= Time.deltaTime * fadeSpeed;
        currentColor.a = Mathf.Clamp01(currentColor.a);

        mat.color = currentColor;

        if (currentColor.a <= 0.01f)
        {
            if (!impactTriggered && crackSpawner != null)
            {
                impactTriggered = true;
            }

            gameObject.SetActive(false);
        }
    }

}