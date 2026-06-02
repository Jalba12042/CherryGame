using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Beach Round", menuName = "Rounds/Beach", order = 1)]
public class BeachRound : Round
{
    [SerializeField] private GameObject[] shellPrefabs;

    public override void setValues()
    {
        base.setValues();
    }
    public override IEnumerator StartGoal()
    {
        return base.StartGoal();
    }

    public override int[] ScoreCount()
    {
        int[] defScores = { 0, 0, 0, 0 };
        return defScores;
    }
}
