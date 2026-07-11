using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Round : ScriptableObject
{
    public List<GameObject> goalObjects;
    public float roundTimeInSeconds;
    public string sceneName;
    public GameObject startTimerUI;
    public bool canHaveEvents;

    [Header("Events excluded from this round")]
    public List<GameEvent> excludedEvents;

    public virtual IEnumerator StartGoal()
    {
        if (startTimerUI == null)
            Debug.LogError($"[Round] '{name}' is missing a StartTimer UI! Tag a GameObject with 'StartTimer' in the scene.");

        yield return null;
    }

    // override this if a round needs to check for stuff during a round
    public virtual void RoundUpdate()
    {
        return;
    }

    public virtual int[] ScoreCount()
    {
        return new int[0];
    }

    public virtual void setValues()
    {
        startTimerUI = GameObject.FindWithTag("StartTimer");

        if (startTimerUI == null)
            Debug.LogError($"[Round] '{name}' could not find a GameObject tagged 'StartTimer' in the scene.");
    }
}