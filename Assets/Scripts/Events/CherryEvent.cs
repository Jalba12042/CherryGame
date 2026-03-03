using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CherryEvent", menuName = "Events/Cherry")]
public class CherryEvent : GameEvent
{
    [SerializeField] private GameObject cherryPrefab;
    public override IEnumerator Trigger()
    {
        isRunning = true;

        GameObject spawner = GameObject.FindWithTag("EventTest");

        if (spawner == null)
        {
            isRunning = false;
            yield break;
        }

        Collider col = spawner.GetComponent<Collider>();

        if (col == null)
        {
            isRunning = false;
            yield break;
        }

        Bounds b = col.bounds;

        float elapsed = 0f;

        while (elapsed < duration && RoundManager.Instance.currRoundActive)
        {
            if (spawner == null)
                break;

            float randX = Random.Range(b.min.x, b.max.x);
            float randZ = Random.Range(b.min.z, b.max.z);

            Instantiate(cherryPrefab, new Vector3(randX, b.min.y, randZ), Quaternion.identity);

            yield return new WaitForSeconds(0.5f);

            elapsed += 0.5f;
        }

        isRunning = false;
    }
}
