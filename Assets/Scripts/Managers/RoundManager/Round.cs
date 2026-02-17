using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Round", menuName = "Rounds/Round", order = 1)]
public class Round : ScriptableObject
{
    public List<GameObject> goalObjects;
    //public List<GameObject> powerupsInPlay;
    public float roundTimeInSeconds;
    public string sceneName;
    public GameObject startTimerUI;

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
