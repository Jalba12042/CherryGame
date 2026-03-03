using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CherryEvent", menuName = "Events/Cherry")]
public class CherryEvent : GameEvent
{
    [SerializeField] private GameObject cherryPrefab;
    public override IEnumerator Trigger()
    {
        isRunning = true;
        GameObject spawner = GameObject.FindWithTag("EventTest");
        Bounds b = spawner.GetComponent<Collider>().bounds;
        float randX = Random.Range(b.min.x, b.max.x);
        float randZ = Random.Range(b.min.z, b.max.z);

        for (int i = 0; i < cooldown; i++)
        {
            Instantiate(cherryPrefab, new Vector3(randX, b.min.y, randZ), Quaternion.identity);
            yield return new WaitForSeconds(.5f);
        }

        isRunning = false;
    }
}
