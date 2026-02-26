using UnityEngine;

public abstract class GameEvent 
{
    public string eventName;
    public float weight;
    public float cooldown;
    public bool canRepeat;

    public abstract void Trigger();
}
