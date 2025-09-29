using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Cherry Round", menuName = "Rounds/Round/Cherry", order = 1)]
public class CherryRound : Round
{
    [SerializeField] private GameObject cherryPrefab;
    [SerializeField] private float spawnInterval;
    [SerializeField] private int minCherrySpawns;
    [SerializeField] private int maxCherrySpawns;

    private GameObject spawnArea;

    public override IEnumerator StartGoal()
    {
        // get the bounds of the area of where we want cherries to spawn
        spawnArea = GameObject.FindWithTag("SpawnArea");
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
                //Debug.Log(randX + " " + spawnArea.transform.position.y + " " + randZ);
                goalObjects.Add(Instantiate(cherryPrefab, new Vector3(randX, spawnArea.transform.position.y, randZ), Quaternion.identity));
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
