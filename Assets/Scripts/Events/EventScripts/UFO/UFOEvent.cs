using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "CherryEvent", menuName = "Events/UFO")]
public class UFOEvent : GameEvent
{
    public GameObject UFOPrefab;
    public Transform[] spawnLocations;

    public override IEnumerator Trigger()
    {
        isRunning = true;
        spawnLocations = FindFirstObjectByType<OffscreenSpawns>().spawns;
        int randSpawn = Random.Range(0, spawnLocations.Length);
        GameObject ufo = Instantiate(UFOPrefab, spawnLocations[randSpawn].position, Quaternion.identity);
        ufo.GetComponent<UFO>().myEvent = this;
        yield return null;
    }
}
