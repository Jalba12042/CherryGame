using System.Collections;
using UnityEngine;

public class CherryRound : Round
{
    [SerializeField] private GameObject cherryPrefab;
    [SerializeField] private Collider spawnArea;
    [SerializeField] private float spawnInterval;
    [SerializeField] private int minCherrySpawns;
    [SerializeField] private int maxCherrySpawns;
    
    public override IEnumerator StartGoal()
    {
        Bounds b = spawnArea.bounds;

        while (RoundManager.Instance.currRoundActive)
        {
            int randCherrySpawns = Random.Range(minCherrySpawns, maxCherrySpawns + 1);

            for (int i = 0; i < randCherrySpawns; i++)
            {
                float randX = Random.Range(b.min.x, b.max.x);
                float randZ = Random.Range(b.min.z, b.max.z);

                Instantiate(cherryPrefab, new Vector3(randX, spawnArea.transform.position.y, randZ), Quaternion.identity);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
