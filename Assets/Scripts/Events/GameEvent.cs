using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public abstract class GameEvent : ScriptableObject
{
    public string eventName;
    public float weight;
    public float cooldown;
    public bool isRunning;
    public float duration;

    public abstract IEnumerator Trigger();
}
