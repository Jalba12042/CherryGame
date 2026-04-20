using System.Collections;
using UnityEngine;

public class PlayerKill : MonoBehaviour
{
    public Renderer[] playerRenderers;
    public bool currDead = false;
    public Playermovement pm;
    [SerializeField] private int respawnDuration;
    public Scream ps;
    private FaceCamStatic myFaceCamStatic;

    private void Awake()
    {
        pm = GetComponent<Playermovement>();
        ps = GetComponentInChildren<Scream>();
        playerRenderers = GetComponentsInChildren<Renderer>();
    }

    private void Start()
    {
        myFaceCamStatic = FaceCamManager.Instance.GetFaceCamStatic(pm.playerIndex);
    }

    /*private void Update()
    {
        // Press X on the keyboard to kill the player manually
        if (Input.GetKeyDown(KeyCode.X))
        {
            killPlayer();
        }
    }*/
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
        currDead = false;
    }

    public void killPlayer()
    {
        GetComponentInChildren<Animator>().enabled = false;
        foreach (var r in playerRenderers)
        {
            r.enabled = false;
        }

        PlayerCherry playerCherry = GetComponent<PlayerCherry>();
        if (playerCherry != null)
            playerCherry.CancelAimAndDrop();

        pm.canMove = false;
        gameObject.layer = LayerMask.NameToLayer("Default");
        transform.position = RoundManager.Instance.currPlayerSpawn.spawnPoints[pm.playerIndex].position;
        ps.enabled = false;
        myFaceCamStatic?.Play();
        currDead = true;

        StartCoroutine(respawnTimer());
    }
}
