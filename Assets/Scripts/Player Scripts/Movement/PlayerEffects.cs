using UnityEngine;
using System.Collections.Generic;

public class PlayerEffects : MonoBehaviour
{
    [Header("Big Impact Settings")]
    public bool isBig = false;
    public float itemJumpForce = 7f;
    private bool wasGroundedLastFrame;
    private Playermovement player;
    private ScreenShake screenShake;

    void Start()
    {
        player = GetComponent<Playermovement>();
        screenShake = FindFirstObjectByType<ScreenShake>();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        if (!wasGroundedLastFrame && player.isGrounded && isBig)
            TriggerBigImpact();

        wasGroundedLastFrame = player.isGrounded;
    }

    private void TriggerBigImpact()
    {
        screenShake?.Shake();

        // Launch nearby objects
        RoundManager rm = RoundManager.Instance;
        ApplyJumpToObjects(rm.currRound.goalObjects);
        ApplyJumpToObjects(rm.powerupsInPlay);

        if (rm.playerObjects != null)
        {
            foreach (var p in rm.playerObjects)
            {
                Playermovement pm = p.GetComponent<Playermovement>();
                if (pm != null && pm.playerIndex != player.playerIndex)
                {
                    Rigidbody rb = p.GetComponent<Rigidbody>();
                    if (rb != null && pm.isGrounded)
                        rb.AddForce(Vector3.up * itemJumpForce, ForceMode.Impulse);
                }
            }
        }
    }

    private void ApplyJumpToObjects(List<GameObject> list)
    {
        if (list == null) return;
        foreach (var obj in list)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddForce(Vector3.up * itemJumpForce, ForceMode.Impulse);
        }
    }
}
