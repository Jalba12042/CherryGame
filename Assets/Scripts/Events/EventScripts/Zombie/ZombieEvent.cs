using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "CherryEvent", menuName = "Events/Zombie")]
public class ZombieEvent : GameEvent
{
    private GameObject zombiePrefab;
    public override IEnumerator Trigger()
    {
        yield return null;
    }
}
