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
        bool rtPressed = false;

        if (player.assignedGamepad != null)
            rtPressed = player.assignedGamepad.rightTrigger.wasPressedThisFrame;
        else if (GameManager.Instance.isOnKeyboard)
            rtPressed = Input.GetKeyDown(KeyCode.E);

        if (rtPressed)
        {
            HandleRT();
        }

        HandleGunInput();
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
        foreach (var kvp in activePowerupInstances)
        {
            Powerup pu = kvp.Value;

            if (pu != null)
            {
                pu.ForceStop();
            }
        }

        activePowerupInstances.Clear();

        // Reset flags
        for (int i = 0; i < currPowerups.Count; i++)
        {
            currPowerups[i] = false;
        }

        isSlowed = false;
        isTased = false;

        // Reset movement
        if (player != null)
        {
            player.moveSpeed = originalMoveSpeed;
        }

        // Turn off VFX
        if (taserVFX != null)
            taserVFX.SetActive(false);

        // Reset UI
        if (FaceCamManager.Instance != null)
            FaceCamManager.Instance.HidePowerUp(player.playerIndex);
    }


    private void HandleGunInput()
    {
        if (activeGun == null)
            return;

        bool pressed = false;

        if (player.assignedGamepad != null)
            pressed = player.assignedGamepad.rightTrigger.wasPressedThisFrame;
        else if (GameManager.Instance.isOnKeyboard)
            pressed = Input.GetKeyDown(KeyCode.E);

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
}