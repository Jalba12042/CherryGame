using System.Collections;
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
            for (int i = 0; i < amountOfZombies; i++)
            {
                float randX = Random.Range(b.min.x, b.max.x);
                float randZ = Random.Range(b.min.z, b.max.z);
                Zombie zombie = Instantiate(zombiePrefab,new Vector3(randX, spawnLocation.transform.position.y, randZ),
                Quaternion.identity).GetComponent<Zombie>();

                zombie.myEvent = this;
                zombie.InitNormalZombie();
            }
        }
        

        yield return new WaitForSeconds(duration);
        isRunning = false;
    }
}
