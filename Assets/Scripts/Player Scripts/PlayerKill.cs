using System.Collections;
using UnityEngine;

public class PlayerKill : MonoBehaviour
{
    public Renderer[] playerRenderers;
    public Playermovement pm;
    [SerializeField] private int respawnDuration;

    private void Awake()
    {
        pm = GetComponent<Playermovement>();
        playerRenderers = GetComponentsInChildren<Renderer>();
    }
    public IEnumerator respawnTimer()
    {
        yield return new WaitForSeconds(respawnDuration);
        pm.canMove = true;
        GetComponentInChildren<Animator>().enabled = true;
        foreach (var r in playerRenderers)
        {
            r.enabled = true;
        }
    }

    public void killPlayer()
    {
        GetComponentInChildren<Animator>().enabled = false;
        foreach (var r in playerRenderers)
        {
            r.enabled = false;
        }

        pm.canMove = false;
        transform.position = RoundManager.Instance.currPlayerSpawn.spawnPoints[pm.playerIndex].position;
        StartCoroutine(respawnTimer());
    }
}
