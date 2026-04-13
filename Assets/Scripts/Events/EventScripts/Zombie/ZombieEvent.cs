using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CherryEvent", menuName = "Events/Zombie")]
public class ZombieEvent : GameEvent
{
    public GameObject zombiePrefab;
    public GameObject spawnLocation;
    public int amountOfZombies = 10;

    public override IEnumerator Trigger()
    {
        isRunning = true;
        spawnLocation = GameObject.FindWithTag("BottomSpawn");
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
                    spawnedPositions.Add(spawnPos);
                }
                else
                {
                    Debug.LogWarning($"Could not find valid spawn for zombie {i} after {maxAttempts} attempts � skipping.");
                }
            }
        }

        yield return new WaitForSeconds(duration);
        isRunning = false;
    }
}
