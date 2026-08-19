using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CherryEvent", menuName = "Events/Zombie")]
public class ZombieEvent : GameEvent
{
    public GameObject zombiePrefab;
    public GameObject spawnLocation;
    public int amountOfZombies = 10;

    [Tooltip("Uncheck for rounds like Mountain where a zombie kill should be a permanent elimination, not a respawn.")]
    public bool respawnOnKill = true;

    [Tooltip("World-space height zombies rise up to in THIS scene. The BottomSpawn zones' own transform height isn't a reliable stand-in for this — tune per round.")]
    public float groundY = 37f;

    public List<GameObject> activeDirtMounds = new List<GameObject>();

    public override IEnumerator Trigger()
    {
        isRunning = true;

        GameObject[] spawnZones = GameObject.FindGameObjectsWithTag("BottomSpawn");

        if (spawnZones.Length > 0)
        {
            List<Vector3> spawnedPositions = new List<Vector3>();

            for (int i = 0; i < amountOfZombies; i++)
            {
                Vector3 spawnPos;
                GameObject chosenZone = null;
                int maxAttempts = 30;
                int attempts = 0;
                bool validPosition = false;

                do
                {
                    // pick a random zone
                    chosenZone = spawnZones[Random.Range(0, spawnZones.Length)];
                    Collider col = chosenZone.GetComponent<Collider>();

                    Bounds b = col.bounds;

                    float randX = Random.Range(b.min.x, b.max.x);
                    float randZ = Random.Range(b.min.z, b.max.z);

                    spawnPos = new Vector3(randX, groundY, randZ);

                    // spacing check
                    validPosition = true;
                    foreach (Vector3 existingPos in spawnedPositions)
                    {
                        if (Vector3.Distance(spawnPos, existingPos) <
                            zombiePrefab.GetComponent<Zombie>().spawnRadius)
                        {
                            validPosition = false;
                            break;
                        }
                    }

                    attempts++;
                }
                while (!validPosition && attempts < maxAttempts);

                if (validPosition)
                {
                    Zombie zombie = Instantiate(zombiePrefab, spawnPos, Quaternion.identity).GetComponent<Zombie>();
                    zombie.myEvent = this;
                    // Rise toward THIS event's configured ground height, not the spawn zone's own
                    // transform Y (which isn't a reliable stand-in for the real terrain surface)
                    zombie.SetGroundY(groundY);
                    zombie.InitNormalZombie();
                    spawnedPositions.Add(spawnPos);
                }
                else
                {
                    Debug.LogWarning($"Could not find valid spawn for zombie {i}, skipping.");
                }
            }
        }

        /*spawnLocation = GameObject.FindWithTag("BottomSpawn");
        if (spawnLocation != null)
        {
            Bounds b = spawnLocation.GetComponent<Collider>().bounds;
            List<Vector3> spawnedPositions = new List<Vector3>();

            for (int i = 0; i < amountOfZombies; i++)
            {
                Vector3 spawnPos;
                int maxAttempts = 30;
                int attempts = 0;
                bool validPosition = false;

                do
                {
                    float randX = Random.Range(b.min.x, b.max.x);
                    float randZ = Random.Range(b.min.z, b.max.z);
                    spawnPos = new Vector3(randX, spawnLocation.transform.position.y, randZ);

                    validPosition = true;
                    foreach (Vector3 existingPos in spawnedPositions)
                    {
                        if (Vector3.Distance(spawnPos, existingPos) < zombiePrefab.GetComponent<Zombie>().spawnRadius)
                        {
                            validPosition = false;
                            break;
                        }
                    }

                    attempts++;
                }
                while (!validPosition && attempts < maxAttempts);

                if (validPosition)
                {
                    Zombie zombie = Instantiate(zombiePrefab, spawnPos, Quaternion.identity).GetComponent<Zombie>();
                    zombie.myEvent = this;
                    zombie.InitNormalZombie();
                    spawnedPositions.Add(spawnPos);
                }
                else
                {
                    Debug.LogWarning($"Could not find valid spawn for zombie {i} after {maxAttempts} attempts � skipping.");
                }
            }
        }*/

        yield return new WaitForSeconds(duration);


        foreach (GameObject dirt in activeDirtMounds)
        {
            if (dirt != null)
                Object.Destroy(dirt);
        }

        activeDirtMounds.Clear();

        isRunning = false;
    }
}
