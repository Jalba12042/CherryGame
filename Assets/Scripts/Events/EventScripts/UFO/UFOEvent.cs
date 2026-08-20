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
            OffscreenSpawns oss = FindFirstObjectByType<OffscreenSpawns>();
            if (oss != null && oss.spawns != null && oss.spawns.Length > 0)
            {
                // isRunning must only go true once a UFO actually exists — UFO.cs is what
                // sets it back to false when it finishes, so if no UFO spawns this would
                // otherwise stay stuck true forever and permanently block all future events.
                isRunning = true;
                spawnLocations = oss.spawns;
                int randSpawn = Random.Range(0, spawnLocations.Length);
                GameObject ufo = Instantiate(UFOPrefab, spawnLocations[randSpawn].position, Quaternion.identity);
                ufo.GetComponent<UFO>().myEvent = this;
            }
            else
            {
                Debug.LogWarning($"[UFOEvent] No OffscreenSpawns (or empty spawns array) found in scene — UFO could not spawn.");
            }
        }
        yield return null;
    }
}
