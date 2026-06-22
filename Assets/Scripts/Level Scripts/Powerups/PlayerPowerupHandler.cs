using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerPowerupHandler : MonoBehaviour
{
    private Playermovement player;
    private Powerup nearbyPowerup;
    private float originalMoveSpeed;
    private bool rtConsumedThisPress;

    [Header("Hand Reference")]
    public Transform handHoldPoint;

    [Header("Powerup State")]
    public List<bool> currPowerups;
    public Dictionary<int, Powerup> activePowerupInstances = new Dictionary<int, Powerup>();

    [Header("Status Flags")]
    public bool isSlowed;
    public bool isTased;

    [Header("Visual Effects")]
    [SerializeField] private GameObject taserVFX;

    [Header("Taser Audio")]
    [SerializeField] private AudioSource taserAudioSource;
    [SerializeField] private AudioClip taserClip;

    [Header("Gun State")]
    public bool hasGunEquipped = false;
    public GunPowerup activeGun;

    [Header("Taco Blaster State")]
    public bool hasTacoBlasterEquipped = false;
    public TacoBlaster activeTacoBlaster;

    [Header("Taco Music")]
    [SerializeField] private AudioSource tacoAudioSource;
    [SerializeField] private AudioClip tacoMusic;

    [HideInInspector] public bool rtConsumedThisFrame = false;
    void Start()
    {
        player = GetComponent<Playermovement>();
        originalMoveSpeed = player.moveSpeed;

        int highestID = -1;
        if (RoundManager.Instance != null && RoundManager.Instance.powerUpsInRotation != null)
        {
            for (int i = 0; i < RoundManager.Instance.powerUpsInRotation.Count; i++)
            {
                Powerup pu = RoundManager.Instance.powerUpsInRotation[i].GetComponent<Powerup>();
                if (pu != null && pu.powerUpID > highestID)
                    highestID = pu.powerUpID;
            }
        }

        currPowerups = new List<bool>();
        for (int i = 0; i <= highestID; i++)
        {
            currPowerups.Add(false);
        }
    }

    void Update()
    {
        rtConsumedThisFrame = false;

        bool rtPressed = InputManager.Instance.GetGrabDown(player.playerID);

        if (rtPressed)
        {
            if (nearbyPowerup != null || activeGun != null || activeTacoBlaster != null)
            {
                rtConsumedThisFrame = true;
                HandleRT();
            }
        }

        HandleGunInput();
        HandleTacoBlasterInput();
    }

    private void HandleRT()
    {
        // pickup gun from ground
        if (nearbyPowerup is GunPowerup gun)
        {
            activeGun = gun;
            nearbyPowerup = null;
            return;
        }

        // equip or fire gun
        if (activeGun != null)
        {
            HandleGunInput();
            return;
        }

        // pickup taco blaster from ground
        if (nearbyPowerup is TacoBlaster taco)
        {
            activeTacoBlaster = taco;
            nearbyPowerup = null;
            return;
        }

        if (activeTacoBlaster != null)
        {
            HandleTacoBlasterInput();
            return;
        }

        if (nearbyPowerup is Taser taser)
        {
            taser.EquipTaser(handHoldPoint);
            nearbyPowerup = null;
            return;
        }

        // fallback: normal powerup
        if (nearbyPowerup != null)
        {
            nearbyPowerup.Activate(this);
            nearbyPowerup = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Powerup powerup))
            nearbyPowerup = powerup;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Powerup powerup) && powerup == nearbyPowerup)
            nearbyPowerup = null;
    }

    public void ApplyPushback(Vector3 direction, float force)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) return;
        Vector3 push = direction.normalized * force;
        rb.AddForce(push, ForceMode.VelocityChange);
    }

    public void ApplySlow(float slowMultiplier, float duration)
    {
        if (isSlowed) return;
        StartCoroutine(SlowRoutine(slowMultiplier, duration));
    }

    private IEnumerator SlowRoutine(float slowMultiplier, float duration)
    {
        isSlowed = true;
        player.moveSpeed *= slowMultiplier;
        yield return new WaitForSeconds(duration);
        player.moveSpeed = originalMoveSpeed;
        isSlowed = false;
    }

    public void ApplyTase(float duration)
    {
        if (isTased) return;
        StartCoroutine(TaseRoutine(duration));
    }

    private IEnumerator TaseRoutine(float duration)
    {
        isTased = true;

        Rigidbody rb = GetComponent<Rigidbody>();

        Vector3 storedVelocity = Vector3.zero;
        RigidbodyConstraints originalConstraints = RigidbodyConstraints.None;

        if (rb != null)
        {
            storedVelocity = rb.linearVelocity;
            originalConstraints = rb.constraints;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        if (taserVFX != null)
            taserVFX.SetActive(true);

        yield return new WaitForSeconds(duration);

        if (rb != null)
        {
            rb.constraints = originalConstraints;
            rb.linearVelocity = storedVelocity;
        }

        if (taserVFX != null)
            taserVFX.SetActive(false);

        isTased = false;
    }

    public void ClearAllPowerups()
    {
        List<Powerup> powerups = new List<Powerup>(activePowerupInstances.Values);

        foreach (Powerup pu in powerups)
        {
            if (pu != null)
            {
                pu.ForceStop();
            }
        }

        activePowerupInstances.Clear();

        for (int i = 0; i < currPowerups.Count; i++)
        {
            currPowerups[i] = false;
        }

        isSlowed = false;
        isTased = false;

        if (player != null)
        {
            player.moveSpeed = originalMoveSpeed;
        }

        if (taserVFX != null)
            taserVFX.SetActive(false);

        if (FaceCamManager.Instance != null)
            FaceCamManager.Instance.HidePowerUp(player.playerIndex);

        if (activeGun != null)
        {
            Destroy(activeGun.gameObject);
            activeGun = null;
        }

        hasGunEquipped = false;

        if (activeTacoBlaster != null)
        {
            Destroy(activeTacoBlaster.gameObject);
            activeTacoBlaster = null;
        }

        hasTacoBlasterEquipped = false;
    }


    private void HandleGunInput()
    {
        if (activeGun == null)
            return;

        bool pressed = InputManager.Instance.GetGrabDown(player.playerID);

        if (!pressed)
            return;

        // EQUIP (first press)
        if (!hasGunEquipped)
        {
            hasGunEquipped = true;

            if (handHoldPoint != null)
                activeGun.EquipGun(handHoldPoint);

            return;
        }

        // FIRE + CONSUME (second press)
        activeGun.Fire();

        hasGunEquipped = false;
        activeGun = null;
    }

    private void HandleTacoBlasterInput()
    {
        if (activeTacoBlaster == null)
            return;

        bool pressed = InputManager.Instance.GetGrabDown(player.playerID);

        if (!pressed)
            return;

        // EQUIP (first press)
        if (!hasTacoBlasterEquipped)
        {
            hasTacoBlasterEquipped = true;

            if (handHoldPoint != null)
                activeTacoBlaster.EquipTacoBlaster(handHoldPoint);

            return;
        }

        // FIRE (second press)
        activeTacoBlaster.Fire();

        hasTacoBlasterEquipped = false;
        activeTacoBlaster = null;
    }

    public void ApplyTacoStun(float duration)
    {
        if (isTased) return;

        StartCoroutine(TacoRoutine(duration));
    }

    private IEnumerator TacoRoutine(float duration)
    {
        isTased = true;

        Rigidbody rb = GetComponent<Rigidbody>();

        Vector3 storedVelocity = Vector3.zero;
        RigidbodyConstraints originalConstraints = RigidbodyConstraints.None;

        if (rb != null)
        {
            storedVelocity = rb.linearVelocity;
            originalConstraints = rb.constraints;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        // Save original volume
        float originalVolume = .15f;

        if (tacoAudioSource != null)
        {
            originalVolume = tacoAudioSource.volume;
        }

        if (GameplayMusicManager.Instance != null)
        {
            GameplayMusicManager.Instance.PauseGameplayMusic();
        }

        // Start taco music
        if (tacoAudioSource != null && tacoMusic != null)
        {
            tacoAudioSource.clip = tacoMusic;
            tacoAudioSource.loop = true;

            // Make louder while dancing
            tacoAudioSource.volume = 1.2f; // Adjust to whatever you want

            tacoAudioSource.Play();
        }

        yield return new WaitForSeconds(duration);

        // Stop music and restore volume
        if (tacoAudioSource != null)
        {
            tacoAudioSource.Stop();
            tacoAudioSource.volume = originalVolume;
        }

        if (GameplayMusicManager.Instance != null)
        {
            GameplayMusicManager.Instance.ResumeGameplayMusic();
        }

        if (rb != null)
        {
            rb.constraints = originalConstraints;
            rb.linearVelocity = storedVelocity;
        }

        isTased = false;
    }
}