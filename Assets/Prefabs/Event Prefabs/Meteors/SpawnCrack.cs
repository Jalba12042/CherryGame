using UnityEngine;
using System.Collections;

public class SpawnCrack : MonoBehaviour
{
    public float growScale = 8f;
    public float growSpeed = 8f;
    public float stayDuration = 2f;
    public float shrinkSpeed = 2f;

    private Vector3 targetScale;

    void Start()
    {
        targetScale = new Vector3(growScale, 1f, growScale);
        transform.localScale = Vector3.zero;
    }

    public void TriggerCrack()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(CrackRoutine());
    }

    IEnumerator CrackRoutine()
    {
        transform.localScale = Vector3.zero;

        // grow
        while (Vector3.Distance(transform.localScale, targetScale) > 0.1f)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                targetScale,
                Time.deltaTime * growSpeed
            );

            yield return null;
        }

        yield return new WaitForSeconds(stayDuration);

        // shrink
        while (transform.localScale.magnitude > 0.1f)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                Vector3.zero,
                Time.deltaTime * shrinkSpeed
            );

            yield return null;
        }

        gameObject.SetActive(false);
    }
}