using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Mounatin Round", menuName = "Rounds/Mountain", order = 1)]
public class MountainRound : Round
{
    private Mountain mountain;

    public override void setValues()
    {
        base.setValues();
        mountain = FindAnyObjectByType<Mountain>();
    }
    public override IEnumerator StartGoal()
    {
        yield return base.StartGoal();

        mountain.StartShrinkLoop();
        while (RoundManager.Instance.currRoundActive)
        {
            yield return null;
        }
    }

    public override int[] ScoreCount()
    {
        GameObject[] players = RoundManager.Instance.playerObjects;
        int[] scores = new int[4];
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null) continue;
            PlayerKill pk = players[i].GetComponentInChildren<PlayerKill>();
            scores[i] = (pk != null && !pk.currDead) ? 1 : 0;
        }
        return scores;
    }
}
