using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Round : ScriptableObject
{
    public List<GameObject> goalObjects;
    //public List<GameObject> powerupsInPlay;
    public float roundTimeInSeconds;
    public string sceneName;
    public GameObject startTimerUI;
    public bool canHaveEvents;

    public virtual IEnumerator StartGoal()
    {
        yield return null;
    }

    public virtual int[] ScoreCount()
    {
        return new int[0];
    }

    public virtual void setValues()
    {
        return;
    }
}
