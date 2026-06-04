using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Beach Round", menuName = "Rounds/Beach", order = 1)]
public class BeachRound : Round
{
    [SerializeField] private GameObject[] shellPrefabs;

    private Ocean ocean;

    public override void setValues()
    {
        base.setValues();

        if (goalObjects == null)
            goalObjects = new List<GameObject>();

        ocean = FindFirstObjectByType<Ocean>();

        if (ocean == null)
            Debug.LogError($"[BeachRound] '{name}' could not find an Ocean object in the scene.");
    }

    public override IEnumerator StartGoal()
    {
        yield return base.StartGoal();

        while (RoundManager.Instance.currRoundActive)
        {
            yield return null;
        }
    }

    public override int[] ScoreCount()
    {
        int[] defScores = { 0, 0, 0, 0 };
        return defScores;
    }
}