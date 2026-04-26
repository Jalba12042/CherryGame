using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerPowerupHandler : MonoBehaviour
{
    private Playermovement player;
    private Powerup nearbyPowerup;
    private float originalMoveSpeed;

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
        if (nearbyPowerup != null && player.assignedGamepad != null)
        {
            if (player.assignedGamepad.rightTrigger.wasPressedThisFrame)
            {
                nearbyPowerup.Activate(this);
                nearbyPowerup = null;
            }
        }
        else if (nearbyPowerup != null && GameManager.Instance.isOnKeyboard)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                nearbyPowerup.Activate(this);
                nearbyPowerup = null;
            }
        }

        HandleGunInput();
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

        if (taserVFX != null)
            taserVFX.SetActive(true);

        if (taserAudioSource != null && taserClip != null)
            taserAudioSource.PlayOneShot(taserClip);

        // --- NEW CLEAN UI LOGIC ---
        if (FaceCamManager.Instance != null) FaceCamManager.Instance.ShowPowerUp(player.playerIndex, "Taser");

        yield return new WaitForSeconds(duration);

        if (taserVFX != null)
            taserVFX.SetActive(false);

        isTased = false;

        // --- NEW CLEAN UI LOGIC ---
        if (FaceCamManager.Instance != null) FaceCamManager.Instance.HidePowerUp(player.playerIndex);
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
        if (activeGun == null) return;

        bool pressed = false;

        if (player.assignedGamepad != null)
        {
            pressed = player.assignedGamepad.rightTrigger.wasPressedThisFrame;
        }
        else if (GameManager.Instance.isOnKeyboard)
        {
            pressed = Input.GetKeyDown(KeyCode.E);
        }

        if (!pressed) return;

        // FIRST PRESS → equip gun
        if (!hasGunEquipped)
        {
            hasGunEquipped = true;

            Transform hand = transform.Find("Hand");
            if (hand != null)
            {
                activeGun.EquipGun(hand);
            }

            return;
        }

        // SECOND PRESS → shoot
        activeGun.Fire();

        // reset so it can't be reused
        hasGunEquipped = false;
        activeGun = null;
    }
}