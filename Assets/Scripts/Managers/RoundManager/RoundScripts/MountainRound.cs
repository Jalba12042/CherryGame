using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Mounatin Round", menuName = "Rounds/Mountain", order = 1)]
public class MountainRound : Round
{
    public override void setValues()
    {
        base.setValues();
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
