using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerCherry : MonoBehaviour
{
    [Header("Throw Settings")]
    public Transform handHoldPoint;

    private Playermovement player;
    private Gamepad gamepad;
    private Animator animator;
    private Projectile projectileScript;
    private GameObject heldCherry;
    private GameObject nearbyCherry;

    void Start()
    {
        player = GetComponent<Playermovement>();
        animator = GetComponent<Animator>();
        projectileScript = GetComponent<Projectile>();
    }

    void Update()
    {
        if (player.assignedGamepad == null) return;
        gamepad = player.assignedGamepad;

        float rtValue = gamepad.rightTrigger.ReadValue();

        if (rtValue > 0.1f)
            HandlePickup();
        else
            HandleDrop();
    }

    private void HandlePickup()
    {
        if (heldCherry == null && nearbyCherry != null)
        {
            heldCherry = nearbyCherry;
            Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
            if (rbCherry != null) rbCherry.isKinematic = true;
            heldCherry.transform.SetParent(handHoldPoint);
            heldCherry.transform.localPosition = Vector3.zero;
            projectileScript?.PickUpCherry(heldCherry);
            if (animator != null)
                StartCoroutine(PlayPickupAnimation());
        }
    }

    private IEnumerator PlayPickupAnimation()
    {
        animator.SetBool("isHoldingCherry", true);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
    }

    private void HandleDrop()
    {
        if (heldCherry != null)
        {
            Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>();
            heldCherry.transform.SetParent(null);
            if (rbCherry != null) rbCherry.isKinematic = false;
            projectileScript?.CancelAim();
            if (animator != null)
                animator.SetBool("isHoldingCherry", false);
            heldCherry = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cherry"))
            nearbyCherry = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Cherry") && other.gameObject == nearbyCherry)
            nearbyCherry = null;
    }
}
