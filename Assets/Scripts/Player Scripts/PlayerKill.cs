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

    [SerializeField] private float respawnBlinkTime = 2f;
    [SerializeField] private float blinkInterval = 0.12f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip deathSound;

    private void Awake()
    {
        pm = GetComponent<Playermovement>();
        ps = GetComponentInChildren<Scream>();
        playerRenderers = GetComponentsInChildren<Renderer>(true);
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
        // Stay completely invisible at the beginning
        foreach (Renderer r in playerRenderers)
            r.enabled = false;

        // Wait until it is time to start blinking
        float invisibleTime = respawnDuration - respawnBlinkTime;

        yield return new WaitForSeconds(invisibleTime);

        // Start blinking for the remaining time
        StartCoroutine(BlinkWhileDead());

        yield return new WaitForSeconds(respawnBlinkTime);

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

        foreach (Renderer r in playerRenderers)
            r.enabled = true;
    }

    public void killPlayer(bool respawn)
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

        //StartCoroutine(BlinkWhileDead());

        PlayerInteract interact = GetComponent<PlayerInteract>();
        if (interact != null)
            interact.CancelAimAndDrop();

        pm.canMove = false;
        gameObject.layer = LayerMask.NameToLayer("Default");
        //transform.position = RoundManager.Instance.currPlayerSpawn.spawnPoints[pm.playerIndex].position;
        ps.enabled = false;
        myFaceCamStatic?.Play();

        transform.position = pm.initialSpawnPosition;

        if (respawn)
            StartCoroutine(respawnTimer());
    }

    void ResetPlayerState()
    {
        GetComponent<Projectile>()?.ForceResetCherry();

        PlayerInteract interact = GetComponent<PlayerInteract>();
        if (interact != null)
        {
            interact.ForceDrop();
            interact.ForceRelease();
        }
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

    IEnumerator BlinkWhileDead()
    {
        bool visible = true;

        while (currDead)
        {
            visible = !visible;

            foreach (Renderer r in playerRenderers)
                r.enabled = visible;

            yield return new WaitForSeconds(blinkInterval);
        }

        // Make sure the player is visible again
        foreach (Renderer r in playerRenderers)
            r.enabled = true;
    }


}
