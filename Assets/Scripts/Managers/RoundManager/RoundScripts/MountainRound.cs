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

    public override void RoundUpdate()
    {
        GameObject[] players = RoundManager.Instance.playerObjects;
        int aliveCount = 0;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null) continue;
            PlayerKill pk = players[i].GetComponentInChildren<PlayerKill>();
            if (pk != null && !pk.currDead) aliveCount++;
        }

        // only cut the round short if it started with more than one player, so solo testing doesn't end instantly
        if (players.Length > 1 && aliveCount <= 1)
        {
            RoundManager.Instance.currRoundProgress = RoundManager.Instance.currRoundDurationInSecs;
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
