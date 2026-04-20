using System.Collections;
using UnityEngine;

public class GoldenCherry : Cherry
{
    [SerializeField] private float waitInAirTime = 1f;
    [SerializeField] private float slowFallDuration = 3f;
    [SerializeField] private float slowFallSpeed = 1f;

    private bool effectApplied = false;
    private GameObject prevPlayerHolding;

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

    private void Update()
    {
        if (playerHolding != null && !effectApplied)
        {
            effectApplied = true;
            prevPlayerHolding = playerHolding;
            PlayerDash pd = playerHolding.GetComponent<PlayerDash>();

            pd.enabled = false;
        }
        else if (!playerHolding && effectApplied) {
            PlayerDash pd = prevPlayerHolding.GetComponent<PlayerDash>();
            pd.enabled = true;
            effectApplied = false;
        }
    }
}
