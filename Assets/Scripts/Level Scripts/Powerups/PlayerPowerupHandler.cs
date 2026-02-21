using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerPowerupHandler : MonoBehaviour
{
    private Playermovement player;
    private Powerup nearbyPowerup;
    private float originalMoveSpeed;

    [Header("Powerup State")]
    public List<bool> currPowerups; // Each index = whether that powerup type is active
    public Dictionary<int, Powerup> activePowerupInstances = new Dictionary<int, Powerup>();

    [Header("Status Flags")]
    public bool isSlowed;
    public bool isTased;

    [Header("Visual Effects")]
    [SerializeField] private GameObject taserVFX;

    [Header("Taser Audio")]
    [SerializeField] private AudioSource taserAudioSource;
    [SerializeField] private AudioClip taserClip;

    void Start()
    {
        player = GetComponent<Playermovement>();
        originalMoveSpeed = player.moveSpeed;

        // --- Initialize currPowerups list based on available powerups in RoundManager ---
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
                nearbyPowerup.Activate(this); // Pass handler instead of Playermovement
                nearbyPowerup = null;
            }
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

    // --- Movement Modifiers (for sprinklers, slows, etc.) ---
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

        yield return new WaitForSeconds(duration);

        if (taserVFX != null)
            taserVFX.SetActive(false);

        isTased = false;
    }
}
