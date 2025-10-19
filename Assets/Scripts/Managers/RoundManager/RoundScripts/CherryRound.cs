using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Cherry Round", menuName = "Rounds/Round/Cherry", order = 1)]
public class CherryRound : Round
{
    [SerializeField] private GameObject cherryPrefab;
    [SerializeField] private GameObject[] powerupPrefabs; // temp power up prefabs
    [SerializeField] private float spawnInterval;
    [SerializeField] private int minCherrySpawns;
    [SerializeField] private int maxCherrySpawns;

    private GameObject spawnArea;
    private GameObject powerupSpawnArea; // temp power up spawn area
    private BasketContainer bc;
    private int[] roundScores;

    public override IEnumerator StartGoal()
    {
        GameObject basketContainer = GameObject.FindWithTag("BasketContainer");
        bc = basketContainer.GetComponent<BasketContainer>();

        // get the bounds of the area of where we want cherries to spawn
        spawnArea = GameObject.FindWithTag("SpawnArea");
        Collider spawnCollider = spawnArea.GetComponent<Collider>();
        Bounds b = spawnCollider.bounds;

        // temp the bounds of power up spawn area
        powerupSpawnArea = GameObject.FindWithTag("PowerUpSpawnArea");
        Collider puSpawnCollider = powerupSpawnArea.GetComponent<Collider>();
        Bounds puB = puSpawnCollider.bounds;

        if (goalObjects == null)
        {
            goalObjects = new List<GameObject>();
        }
        // while the round is going, spawn a random amount of cherry prefabs in random spots in the spawn area every n seconds
        while (RoundManager.Instance.currRoundActive)
        {
            int randCherrySpawns = Random.Range(minCherrySpawns, maxCherrySpawns + 1);
            int randPUSpawns = Random.Range(0, 2); // temp power up random amount

            for (int i = 0; i < randCherrySpawns; i++)
            {
                float randX = Random.Range(b.min.x, b.max.x);
                float randZ = Random.Range(b.min.z, b.max.z);
                goalObjects.Add(Instantiate(cherryPrefab, new Vector3(randX, spawnArea.transform.position.y, randZ), Quaternion.identity));
            }
            
            // temp power up spawn logic
            for (int i = 0; i < randPUSpawns; i++)
            {
                float randX = Random.Range(puB.min.x, puB.max.x);
                float randZ = Random.Range(puB.min.z, puB.max.z);

                Instantiate(powerupPrefabs[Random.Range(0, powerupPrefabs.Length)], new Vector3(randX, powerupSpawnArea.transform.position.y, randZ), Quaternion.identity);
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public override int[] ScoreCount()
    {
        return bc.countCherries();
    }
}
