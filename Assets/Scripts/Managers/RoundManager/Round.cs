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

    public virtual IEnumerator StartGoal()
    {
        if (startTimerUI == null)
            Debug.LogError($"[Round] '{name}' is missing a StartTimer UI! Tag a GameObject with 'StartTimer' in the scene.");

        yield return null;
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