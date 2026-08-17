using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "CherryEvent", menuName = "Events/UFO")]
public class UFOEvent : GameEvent
{
    public GameObject UFOPrefab;
    public Transform[] spawnLocations;

    [Tooltip("Uncheck for rounds like Mountain where an abduction should be a permanent elimination, not a respawn.")]
    public bool respawnOnKill = true;

    public override IEnumerator Trigger()
    {
        if (RoundManager.Instance.currRoundActive) {
            isRunning = true;
            OffscreenSpawns oss = FindFirstObjectByType<OffscreenSpawns>();
            if (oss != null)
            {
                spawnLocations = FindFirstObjectByType<OffscreenSpawns>().spawns;
                int randSpawn = Random.Range(0, spawnLocations.Length);
                GameObject ufo = Instantiate(UFOPrefab, spawnLocations[randSpawn].position, Quaternion.identity);
                ufo.GetComponent<UFO>().myEvent = this;
            }
        }
        yield return null;
    }
}
