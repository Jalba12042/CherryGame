using UnityEngine;
using System.Collections;

public class SpawnCrack : MonoBehaviour
{
    public Vector3 targetScale = new Vector3(35.15f, 3.13f, 35.15f);


    public float growScale = 8f;
    public float growSpeed = 8f;
    public float stayDuration = 2f;
    public float shrinkSpeed = 2f;

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
        while (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
        {
            transform.localScale = Vector3.MoveTowards(
                transform.localScale,
                targetScale,
                growSpeed * Time.deltaTime
            );

            yield return null;
        }

        yield return new WaitForSeconds(stayDuration);

        // shrink
        while (transform.localScale.magnitude > 0.1f)
        {
            transform.localScale = Vector3.MoveTowards(
                transform.localScale,
                Vector3.zero,
                shrinkSpeed * Time.deltaTime
            );

            yield return null;
        }

        gameObject.SetActive(false);
    }
}