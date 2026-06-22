using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "CherryEvent", menuName = "Events/Meteor")]
public class MeteorEvent : GameEvent
{
    public GameObject meteorPrefab;
    public override IEnumerator Trigger()
    {
        isRunning = true;

        GameObject spawner = GameObject.FindWithTag("UpperSpawn");

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

            Instantiate(meteorPrefab, new Vector3(randX, b.min.y, randZ), Quaternion.identity);

            yield return new WaitForSeconds(1f);

            elapsed += 1;
        }

        isRunning = false;
    }
}
