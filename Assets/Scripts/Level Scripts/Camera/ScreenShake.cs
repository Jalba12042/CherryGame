using System.Collections;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float defaultDuration = 0.3f;
    [SerializeField] private float defaultMagnitude = 1f;

    private Vector3 originalPos;

    private void Awake()
    {
        originalPos = transform.localPosition;
    }

    public void Shake()
    {
        StartCoroutine(ShakeCoroutine(defaultDuration, defaultMagnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
