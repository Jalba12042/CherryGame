using UnityEngine;

public class CherryEvent : GameEvent
{
    [SerializeField] private GameObject cherryPrefab;
    [SerializeField] private int maxCherrySpawns;
    public override void Trigger()
    {
        GameObject spawner = GameObject.FindWithTag("EventTest");
        Bounds b = spawner.GetComponent<Collider>().bounds;
        float randX = Random.Range(b.min.x, b.max.x);
        float randZ = Random.Range(b.min.z, b.max.z);

        for (int i = 0; i < maxCherrySpawns; i++)
        {
            Instantiate(cherryPrefab, new Vector3(randX, b.min.y, randZ), Quaternion.identity);
        }
    }
}
