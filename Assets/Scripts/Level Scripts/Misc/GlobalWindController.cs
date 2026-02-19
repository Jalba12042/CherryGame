using UnityEngine;
using System.Collections;

public class GlobalWindController : MonoBehaviour
{
    [Header("Wind Settings")]
    [SerializeField] private float minTimeBetweenWind = 10f;
    [SerializeField] private float maxTimeBetweenWind = 25f;

    [SerializeField] private float windDuration = 5f;
    [SerializeField] private float windForce = 20f;

    [SerializeField] private Vector3 windDirection = Vector3.right;

    private void Start()
    {
        StartCoroutine(WindRoutine());
    }

    private IEnumerator WindRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minTimeBetweenWind, maxTimeBetweenWind);
            yield return new WaitForSeconds(waitTime);

            yield return StartCoroutine(ApplyWind());
        }
    }

    private IEnumerator ApplyWind()
    {
        Debug.Log("Wind Started");

        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;

        float timer = 0f;

        while (timer < windDuration)
        {
            ApplyForceToScene(randomDirection);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        Debug.Log("Wind Ended");
    }

    private void ApplyForceToScene(Vector3 direction)
    {
        Rigidbody[] bodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);

        foreach (Rigidbody rb in bodies)
        {
            if (!rb.CompareTag("Player"))
            {
                rb.AddForce(direction * windForce, ForceMode.Force);
            }
        }
    }
}
