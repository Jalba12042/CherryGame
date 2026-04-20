using System.Collections;
using UnityEngine;

public class GoldenCherry : Cherry
{
    [SerializeField] private float waitInAirTime = 1f;
    [SerializeField] private float slowFallDuration = 3f;
    [SerializeField] private float slowFallSpeed = 1f;

    private void Awake()
    {
        StartCoroutine(Spawn());
    }
    private IEnumerator Spawn()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        float elapsed = 0f;

        while (elapsed < slowFallDuration)
        {
            transform.position += Vector3.down * slowFallSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(waitInAirTime);

        rb.isKinematic = false;
    }
}
