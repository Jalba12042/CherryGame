using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Beach Round", menuName = "Rounds/Beach", order = 1)]
public class BeachRound : Round
{
    [SerializeField] private GameObject[] shellPrefabs;

    [SerializeField] private float spawnInterval;
    [SerializeField, Range(0, 100)] private int powerupSpawnRate;
    [SerializeField] private PowerupSpawner powerupSpawner;

    private Ocean ocean;
    private BasketContainer bc;

    public override void setValues()
    {
        base.setValues();

        if (goalObjects == null)
            goalObjects = new List<GameObject>();

        ocean = FindFirstObjectByType<Ocean>();
        if (ocean == null)
            Debug.LogError($"[BeachRound] '{name}' could not find an Ocean object in the scene.");

        bc = FindFirstObjectByType<BasketContainer>();
        if (bc == null)
            Debug.LogError($"[BeachRound] '{name}' could not find a BasketContainer in the scene.");

        powerupSpawner.Init();
    }

    public override IEnumerator StartGoal()
    {
        yield return base.StartGoal();

        while (RoundManager.Instance.currRoundActive)
        {
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
        if (bc != null) return bc.countCherries();
        return new int[4];
    }
}