using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


[CreateAssetMenu(fileName = "New Cherry Round", menuName = "Rounds/Cherry", order = 1)]
public class CherryRound : Round
{
    [SerializeField] private GameObject cherryPrefab;
    [SerializeField] private GameObject cherryBombPrefab;
    [SerializeField] private GameObject goldenCherryPrefab;
    [SerializeField] private float spawnInterval;
    [SerializeField] private int minCherrySpawns;
    [SerializeField] private int maxCherrySpawns;
    [SerializeField] private float cherryBombChance;
    [SerializeField] private float goldenCherryChance = 0.1f;
    [SerializeField, Range(0, 100)] private int powerupSpawnRate;
    [SerializeField] private PowerupSpawner powerupSpawner;

    private GameObject spawnArea;
    private BasketContainer bc;

    public override void setValues()
    {
        base.setValues();

        GameObject basketContainer = GameObject.FindWithTag("BasketContainer");
        bc = basketContainer.GetComponent<BasketContainer>();

        // get the bounds of the area of where we want cherries to spawn
        spawnArea = GameObject.FindWithTag("SpawnArea");

        powerupSpawner.Init();

        if (goalObjects == null)
        {
            goalObjects = new List<GameObject>();
        }
    }
    public override IEnumerator StartGoal()
    {
        Collider spawnCollider = spawnArea.GetComponent<Collider>();
        Bounds b = spawnCollider.bounds;

        // while the round is going, spawn a random amount of cherry prefabs in random spots in the spawn area every n seconds
        while (RoundManager.Instance.currRoundActive)
        {
            int randCherrySpawns = Random.Range(minCherrySpawns, maxCherrySpawns + 1);

            for (int i = 0; i < randCherrySpawns; i++)
            {
                float randX = Random.Range(b.min.x, b.max.x);
                float randZ = Random.Range(b.min.z, b.max.z);
                Vector3 spawnPos = new Vector3(randX, spawnArea.transform.position.y, randZ);

                // roll which cherry variant spawns in this slot: golden (rare), bomb, or a plain cherry
                float randCherryType = Random.Range(0f, 1f);

                if (randCherryType < goldenCherryChance)
                    goalObjects.Add(Instantiate(goldenCherryPrefab, spawnPos, Quaternion.identity));
                else if (randCherryType < goldenCherryChance + cherryBombChance)
                    goalObjects.Add(Instantiate(cherryBombPrefab, spawnPos, Quaternion.identity));
                else
                    goalObjects.Add(Instantiate(cherryPrefab, spawnPos, Quaternion.identity));
            }

            // powerup spawn logic
            int randPUSpawn = Random.Range(0, 100);
            if (randPUSpawn <= powerupSpawnRate)
            {
                powerupSpawner.SpawnCrate();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public override int[] ScoreCount()
    {
        return bc.countCherries();
    }

    public void SetBasketContainer(BasketContainer container)
    {
        bc = container;
    }

}
