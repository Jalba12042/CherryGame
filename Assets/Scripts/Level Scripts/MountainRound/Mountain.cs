using System.Collections;
using UnityEngine;

public class Mountain : MonoBehaviour
{
    [Header("Shrink Timing")]
    [Tooltip("Seconds between each shrink event.")]
    [SerializeField] private float shrinkInterval = 15f;

    [Tooltip("How long each shrink takes to animate, in seconds.")]
    [SerializeField] private float shrinkDuration = 3f;

    [Header("Shrink Amount")]
    [Tooltip("Multiplier applied to X/Z size each shrink event (0.75 = shrinks to 75% of current size).")]
    [Range(0.1f, 0.95f)]
    [SerializeField] private float shrinkFactor = 0.75f;

    [Tooltip("Smallest the platform is allowed to get on X/Z.")]
    [SerializeField] private float minSize = 2f;

    public void StartShrinkLoop()
    {
        StartCoroutine(ShrinkLoop());
    }

    private IEnumerator ShrinkLoop()
    {
        // Keep shrinking until we hit the floor size.
        while (transform.localScale.x > minSize)
        {
            yield return new WaitForSeconds(shrinkInterval);

            Vector3 startScale = transform.localScale;
            Vector3 targetScale = new Vector3(
                Mathf.Max(startScale.x * shrinkFactor, minSize),
                startScale.y, // height stays fixed - only the footprint shrinks
                Mathf.Max(startScale.z * shrinkFactor, minSize)
            );

            float elapsed = 0f;
            while (elapsed < shrinkDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / shrinkDuration;
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            // Snap to exact target in case of frame timing drift.
            transform.localScale = targetScale;
        }
    }
}
