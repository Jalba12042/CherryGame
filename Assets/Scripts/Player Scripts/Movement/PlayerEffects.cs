using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerEffects : MonoBehaviour
{
    [Header("Big Impact Settings")]
    public bool isBig = false;
    public float itemJumpForce = 100f;
    private bool wasGroundedLastFrame;
    private Playermovement player;
    private ScreenShake screenShake;
    private EnvironmentEffects envEffects;
    private GroundCheck gc;

    private Rigidbody rb;

    [Header("Taser Settings")]
    [SerializeField] private float taseForce;
    [SerializeField] private float stunDuration;
    public bool isTasing;

    void Start()
    {
        player = GetComponent<Playermovement>();
        screenShake = FindFirstObjectByType<ScreenShake>();
        envEffects = FindFirstObjectByType<EnvironmentEffects>();
        rb = GetComponent<Rigidbody>();
        gc = GetComponent<GroundCheck>();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        if (!wasGroundedLastFrame && gc.isGrounded && isBig)
            TriggerBigImpact();

        wasGroundedLastFrame = gc.isGrounded;
    }

    /*private void TriggerBigImpact()
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
    }*/

    private void TriggerBigImpact()
    {
        screenShake?.Shake();
        envEffects?.bigImpact(itemJumpForce, rb);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && isTasing)
        {
            Playermovement otherChar = collision.gameObject.GetComponent<Playermovement>();

            if (otherChar != null && otherChar.canMove)
            {
                Debug.Log("collision!");

                Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();

                //Get their PlayerPowerupHandler
                PlayerPowerupHandler otherHandler =
                    collision.gameObject.GetComponent<PlayerPowerupHandler>();

                if (otherHandler != null)
                {
                    otherHandler.ApplyTase(stunDuration);
                }

                StartCoroutine(Stun(stunDuration, rb, otherChar));
            }
        }

        if (collision.gameObject.CompareTag("Player") && isBig)
        {
            PlayerKill pk = collision.gameObject.GetComponent<PlayerKill>();

            if (pk != null)
            {
                pk.killPlayer(true);
            }
        }
    }
    public IEnumerator Stun(float duration, Rigidbody rb, Playermovement pm)
    {
        pm.canMove = false;
        if (rb != null)
        {
            Vector3 direction = (pm.gameObject.transform.position - transform.position).normalized;
            direction.y = 0.5f; // add a little upward push if you want
            rb.AddForce(direction * taseForce, ForceMode.Impulse);
        }
        yield return new WaitForSeconds(duration);
        pm.canMove = true;
    }
}
