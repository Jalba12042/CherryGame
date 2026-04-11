using UnityEngine;

public class MeteorShadowExpand : MonoBehaviour
{
 
    public Transform fallingObject;   // Object moving toward the ground
    public float maxHeight = 20f;     // Height where scale starts at 20
    public float minHeight = 0f;      // Ground level
    public float minScale = 20f;
    public float maxScale = 50f;

    void Update()
    {
        float height = fallingObject.position.y;

        // Convert height into 0-1 value
        float t = Mathf.InverseLerp(maxHeight, minHeight, height);

        // Scale between 20 and 50
        float scale = Mathf.Lerp(minScale, maxScale, t);

        transform.localScale = new Vector3(scale, scale, scale);
    }
}

