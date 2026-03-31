using System.Collections;
using UnityEngine;

public class PlayerKill : MonoBehaviour
{
    public Renderer[] playerRenderers;
    public Playermovement pm;
    [SerializeField] private int respawnDuration;
    public Scream ps;
    private FaceCamStatic myFaceCamStatic;

    private void Awake()
    {
        pm = GetComponent<Playermovement>();
        ps = GetComponentInChildren<Scream>();
        playerRenderers = GetComponentsInChildren<Renderer>();
        myFaceCamStatic = FaceCamManager.Instance.GetFaceCamStatic(pm.playerIndex);

    }

    private void Update()
    {
        // Press X on the keyboard to kill the player manually
        if (Input.GetKeyDown(KeyCode.X))
        {
            killPlayer();
        }
    }
    public IEnumerator respawnTimer()
    {
        yield return new WaitForSeconds(respawnDuration);
        myFaceCamStatic?.Stop();

        pm.canMove = true;
        gameObject.layer = LayerMask.NameToLayer("Player");
        GetComponentInChildren<Animator>().enabled = true;
        foreach (var r in playerRenderers)
        {
            r.enabled = true;
        }
        ps.enabled = true;
    }

    public void killPlayer()
    {
        GetComponentInChildren<Animator>().enabled = false;
        foreach (var r in playerRenderers)
        {
            r.enabled = false;
        }

        pm.canMove = false;
        gameObject.layer = LayerMask.NameToLayer("Default");
        transform.position = RoundManager.Instance.currPlayerSpawn.spawnPoints[pm.playerIndex].position;
        ps.enabled = false;
        myFaceCamStatic?.Play();

        StartCoroutine(respawnTimer());
    }
}
