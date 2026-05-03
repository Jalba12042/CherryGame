using System.Collections;
using UnityEngine;

public class PlayerKill : MonoBehaviour
{
    public Renderer[] playerRenderers;

    [SerializeField] private GameObject visualRoot;

    public bool currDead = false;
    public Playermovement pm;
    [SerializeField] private int respawnDuration;
    public Scream ps;
    private FaceCamStatic myFaceCamStatic;

    [SerializeField] private GameObject confettiPrefab;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip deathSound;

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

        RespawnPlayer();
    }

    void RespawnPlayer()
    {
        // ensure clean position first
        transform.position = pm.initialSpawnPosition;

        // restore physics + collisions
        UnfreezePlayer();

        // restore control/state
        pm.canMove = true;
        gameObject.layer = LayerMask.NameToLayer("Player");

        // visuals back
        GetComponentInChildren<Animator>().enabled = true;

        visualRoot.SetActive(true);

        foreach (var r in playerRenderers)
            r.enabled = true;

        ps.enabled = true;

        myFaceCamStatic?.Stop();

        currDead = false;
    }

    public void killPlayer()
    {
        if (currDead) return;

        currDead = true;

        // --- PLAY DEATH SOUND ---
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        Vector3 deathPosition = transform.position;

        // Spawn confetti
        if (confettiPrefab != null)
        {
            Instantiate(confettiPrefab, deathPosition, Quaternion.identity);
        }

        ResetPlayerState();

        PlayerPowerupHandler powerHandler = GetComponent<PlayerPowerupHandler>();
        if (powerHandler != null)
        {
            powerHandler.ClearAllPowerups();
        }

        FreezePlayer();

        GetComponentInChildren<Animator>().enabled = false;

        visualRoot.SetActive(false);
        foreach (var r in playerRenderers)
        {
            r.enabled = false;
        }

        PlayerCherry playerCherry = GetComponent<PlayerCherry>();
        if (playerCherry != null)
            playerCherry.CancelAimAndDrop();

        pm.canMove = false;
        gameObject.layer = LayerMask.NameToLayer("Default");
        //transform.position = RoundManager.Instance.currPlayerSpawn.spawnPoints[pm.playerIndex].position;
        ps.enabled = false;
        myFaceCamStatic?.Play();

        transform.position = pm.initialSpawnPosition;

        StartCoroutine(respawnTimer());
    }

    void ResetPlayerState()
    {
        // Cherry system full reset
        GetComponent<PlayerCherry>()?.ForceDrop();
        GetComponent<Projectile>()?.ForceResetCherry();

        // Drop someone you're holding
        GetComponent<PlayerGrab>()?.ForceRelease();

        // Break out if you're being held
        GetComponent<PlayerGrabbed>()?.ReleaseGrabbedPlayer();
    }

    void FreezePlayer()
    {
        pm.canMove = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    void UnfreezePlayer()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = true;

        pm.canMove = true;
    }
}
