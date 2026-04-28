using Assets.DuckType.Jiggle;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrab : MonoBehaviour
{
    public Transform pickupTarget;
    public float pickupRange = 2f;
    public float grabCooldownTime = 1f;

    public Playermovement player;
    private Gamepad gamepad;
    private GameObject grabbedPlayer;
    private Rigidbody grabbedRigidbody;
    private Collider myCollider, grabbedCollider;
    private bool canGrab = true;

    private GameObject nearbyPlayer;
    [HideInInspector] public bool isGrabbed = false; // new flag for grabbed state

    private bool grabEscapeCooldown = false; // prevents grab right after escape
    public float escapeCooldownTime = 1f;    // duration of cooldown after escape
    private Animator animator;
    [SerializeField] private Animator faceAnimator;
    public GameObject stunCanvas;
    public TMPro.TMP_Text stunTimerText;

    private Jiggle[] myJiggles;

    [Header("Grab Audio")]
    [SerializeField] private AudioSource grabSource;
    [SerializeField] private AudioClip grabClip;
    [SerializeField] private AudioClip dropClip;

    [Header("Throw Settings")]
    [SerializeField] private float throwForceForward = 25f;
    [SerializeField] private float throwForceUp = 12f;

    private void Awake()
    {
        if (faceAnimator == null)
            faceAnimator = GetComponentInChildren<Animator>();
    }


    void Start()
    {
        player = GetComponent<Playermovement>();
        myCollider = GetComponent<Collider>();
        animator = GetComponent<Animator>();
        myJiggles = GetComponentsInChildren<Jiggle>();

        if (stunCanvas != null)
            stunCanvas.SetActive(false);

    }

    void Update()
    {
        if (!GameManager.Instance.isOnKeyboard)
            gamepad = player.assignedGamepad;

        // Keep grabbed player at pickup target
        if (grabbedPlayer != null && pickupTarget != null)
        {
            grabbedPlayer.transform.position = pickupTarget.position;
            grabbedPlayer.transform.rotation = pickupTarget.rotation;
        }

        if (grabEscapeCooldown) return;

        if (!GameManager.Instance.isOnKeyboard)
        {
            if (gamepad == null) return;

            // RT TAP → toggle grab/release
            if (gamepad.rightTrigger.wasPressedThisFrame)
            {
                if (grabbedPlayer == null)
                    TryGrab();
                else
                    ReleaseGrab();
            }

            // LT RELEASE → throw (UNCHANGED)
            if (gamepad.leftTrigger.wasReleasedThisFrame && grabbedPlayer != null)
            {
                ThrowGrabbedPlayer();
            }
        }
        else
        {
            // Keyboard version (same logic)
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (grabbedPlayer == null)
                    TryGrab();
                else
                    ReleaseGrab();
            }

            // Throw (UNCHANGED)
            if (Input.GetKeyUp(KeyCode.Q) && grabbedPlayer != null)
            {
                ThrowGrabbedPlayer();
            }
        }
    }

    public bool TryGrab()
    {
        if (!canGrab || grabbedPlayer != null || nearbyPlayer == null || isGrabbed)
            return false;

        /*float distance = Vector3.Distance(transform.position, nearbyPlayer.transform.position);
        if (distance > pickupRange)
        {
            nearbyPlayer = null; // clear stale reference
            return false;
        }*/


        PlayerEffects pe = nearbyPlayer.GetComponent<PlayerEffects>();
        if (pe != null && pe.isBig)
            return false;

        grabbedPlayer = nearbyPlayer;

        animator.SetBool("isGrabbing", true);

        if (grabSource != null && grabClip != null)
        {
            grabSource.pitch = Random.Range(0.95f, 1.05f); // slight variation
            grabSource.PlayOneShot(grabClip);
        }

        grabbedRigidbody = grabbedPlayer.GetComponent<Rigidbody>();
        if (grabbedRigidbody != null)
            grabbedRigidbody.isKinematic = true;

        grabbedCollider = grabbedPlayer.GetComponent<Collider>();
        if (grabbedCollider != null)
            Physics.IgnoreCollision(myCollider, grabbedCollider, true);

        Playermovement pm = grabbedPlayer.GetComponent<Playermovement>();
        if (pm != null)
            pm.canMove = false;

        PlayerGrab grabbedGrab = grabbedPlayer.GetComponent<PlayerGrab>();
        if (grabbedGrab != null)
            grabbedGrab.isGrabbed = true;

        PlayerGrabbed grabbedScript = grabbedPlayer.GetComponent<PlayerGrabbed>();
        if (grabbedScript != null)
        {
            grabbedScript.grabber = this;
        }

        Animator grabbedAnimator = grabbedPlayer.GetComponent<Animator>();
        if (grabbedAnimator != null)
            grabbedAnimator.SetBool("isGrabbed", true);

        PlayerEscapeUI escapeUI = grabbedPlayer.GetComponent<PlayerEscapeUI>();
        if (escapeUI != null)
        {
            escapeUI.StartBeingGrabbed(this);
        }

        
        SetMyJiggle(false);
        return true;
    }


    public void ReleaseGrab()
    {
        if (grabbedPlayer == null) return;

        if (grabSource != null && dropClip != null)
        {
            grabSource.pitch = Random.Range(0.9f, 1.0f);
            grabSource.PlayOneShot(dropClip);
        }

        if (grabbedRigidbody != null)
            grabbedRigidbody.isKinematic = false;

        if (grabbedCollider != null)
            Physics.IgnoreCollision(myCollider, grabbedCollider, false);

        if (grabbedPlayer != null)
        {
            Playermovement pm = grabbedPlayer.GetComponent<Playermovement>();
            if (pm != null)
                pm.canMove = true; // restore movement

            // Stop Escape UI for grabbed player
            var escapeUI = grabbedPlayer.GetComponent<PlayerEscapeUI>();
            if (escapeUI != null)
                escapeUI.StopBeingGrabbed();

            // Reset grabbed player's grab flag
            PlayerGrab grabbedGrab = grabbedPlayer.GetComponent<PlayerGrab>();
            if (grabbedGrab != null)
            {
                grabbedGrab.isGrabbed = false;
                grabbedGrab.enabled = true;
            }

            PlayerGrabbed grabbedScript = grabbedPlayer.GetComponent<PlayerGrabbed>();
            if (grabbedScript != null)
            {
                grabbedScript.grabber = null;
            }

            Animator grabbedAnimator = grabbedPlayer.GetComponent<Animator>();
            if (grabbedAnimator != null)
                grabbedAnimator.SetBool("isGrabbed", false);

            if (animator != null)
                animator.SetBool("isGrabbing", false); // set false when released


        }
        SetMyJiggle(true);

        grabbedPlayer = null;
        grabbedRigidbody = null;
        grabbedCollider = null;

        StartCoroutine(GrabCooldown());
    }

    private void ThrowGrabbedPlayer()
    {
        if (grabbedPlayer == null || grabbedRigidbody == null) return;

        GameObject thrownPlayer = grabbedPlayer;
        Rigidbody thrownRb = grabbedRigidbody;
        PlayerGrab thrownGrab = grabbedPlayer.GetComponent<PlayerGrab>();
        Playermovement thrownPm = grabbedPlayer.GetComponent<Playermovement>();

        ReleaseGrab();

        if (animator != null)
            animator.SetBool("isPickingUp", false);

        Animator thrownAnimator = thrownPlayer.GetComponent<Animator>();
        if (thrownAnimator != null)
            thrownAnimator.SetBool("isGrabbed", false);

        // Apply throw force using Inspector variables
        thrownRb.isKinematic = false;
        thrownRb.linearVelocity = Vector3.zero; // reset any previous velocity
        Vector3 throwDir = transform.forward;
        thrownRb.AddForce(throwDir * throwForceForward + Vector3.up * throwForceUp, ForceMode.Impulse);

        if (thrownPm != null)
            StartCoroutine(thrownPm.GetComponent<PlayerGrab>().StunRoutine(3f));

        if (thrownGrab != null)
            thrownGrab.enabled = true;
    }


    public IEnumerator StunRoutine(float duration)
    {

        player.canMove = false;

        animator.SetBool("isStunned", true);

        // Freeze random blinking
        RandomFaceChanger rfc = GetComponentInChildren<RandomFaceChanger>();
        if (rfc != null)
            rfc.PauseFaces();

        if (faceAnimator != null)
            faceAnimator.SetBool("isStunned", true);

        // ----- ENABLE UI -----
        if (stunCanvas != null)
            stunCanvas.SetActive(true);

        float remaining = duration;

        while (remaining > 0f)
        {
            if (stunTimerText != null)
                stunTimerText.text = Mathf.Ceil(remaining).ToString(); // 3,2,1

            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }

        // Turn off stunned animation
        if (faceAnimator != null)
            faceAnimator.SetBool("isStunned", false);

        // Resume blinking
        if (rfc != null)
            rfc.ResumeFaces();

        // ----- DISABLE UI -----
        if (stunCanvas != null)
            stunCanvas.SetActive(false);

        player.canMove = true;

        animator.SetBool("isStunned", false);
    }





    private void OnTriggerEnter(Collider other)
    {
        Playermovement pm = other.GetComponent<Playermovement>();
        if (pm != null && pm.playerIndex != player.playerIndex)
        {
            nearbyPlayer = other.gameObject;
        }
    }

    /*private void OnTriggerEnter(Collider other)
    {
        Playermovement pm = other.GetComponent<Playermovement>();
        if (pm != null && pm.playerIndex != player.playerIndex)
        {
            // Only set if not already holding someone closer
            if (nearbyPlayer == null)
                nearbyPlayer = other.gameObject;
            else
            {
                // Pick whoever is closer
                float distNew = Vector3.Distance(transform.position, other.transform.position);
                float distCurrent = Vector3.Distance(transform.position, nearbyPlayer.transform.position);
                if (distNew < distCurrent)
                    nearbyPlayer = other.gameObject;
            }
        }
    }*/

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == nearbyPlayer)
            nearbyPlayer = null;
    }

    public IEnumerator GrabCooldown()
    {
        canGrab = false;
        yield return new WaitForSeconds(grabCooldownTime);
        canGrab = true;
    }

    public void ForceRelease()
    {
        ReleaseGrab();

        StartCoroutine(EscapeGrabCooldown());
    }

    private IEnumerator EscapeGrabCooldown()
    {
        grabEscapeCooldown = true;
        yield return new WaitForSeconds(escapeCooldownTime);
        grabEscapeCooldown = false;
    }

    void SetMyJiggle(bool enabled)
    {
        if (myJiggles == null) return;

        foreach (var jiggle in myJiggles)
        {
            if (jiggle != null)
                jiggle.enabled = enabled;
        }
    }

    public void ForceReleaseAll()
    {
        // If I'm holding someone → release them
        if (grabbedPlayer != null)
        {
            ReleaseGrab();
        }

        // If I'm being held → force the OTHER player to release me
        if (isGrabbed)
        {
            PlayerGrab other = FindObjectOfType<PlayerGrab>(); // better: track who grabbed you
            if (other != null)
            {
                other.ReleaseGrab();
            }

            isGrabbed = false;
        }
    }
}
