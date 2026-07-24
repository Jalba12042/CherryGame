using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "MagicMirrorEvent", menuName = "Events/Magic Mirror")]
public class MagicMirrorEvent : GameEvent
{
    public override IEnumerator Trigger()
    {
        isRunning = true;

        Playermovement[] players = FindObjectsByType<Playermovement>(FindObjectsSortMode.None);

        // Reverse controls ON
        foreach (Playermovement player in players)
        {
            player.controlsReversed = true;

            PlayerEffects effects =
                player.GetComponent<PlayerEffects>();

            if (effects != null)
            {
                effects.SetDizzyVFX(true);
            }
        }

        float elapsed = 0f;

        while (elapsed < duration && RoundManager.Instance.currRoundActive)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reverse controls OFF
        foreach (Playermovement player in players)
        {
            if (player != null)
            {
                player.controlsReversed = false;

                PlayerEffects effects =
                player.GetComponent<PlayerEffects>();

                if (effects != null)
                {
                    effects.SetDizzyVFX(false);
                }
            }
        }

        isRunning = false;
    }
}