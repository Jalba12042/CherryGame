using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
public class PlayerMovement : MonoBehaviour 
{ 
    [Header("Player Settings")] 
    public int playerIndex = 0; // Set this when the player is spawned
    public float moveSpeed = 5f; 
    public float jumpForce = 7f; 
    
    [Header("Throw Settings")] 
    public Transform handHoldPoint; // Where cherry sits
    public GameObject cherryPrefab; // Prefab for throwing

    [Header("Ground Check")] 
    [SerializeField] private Transform groundCheckPoint; 
    [SerializeField] private float groundCheckDistance = 0.4f; 
    [SerializeField] private LayerMask groundLayer; 


    private Gamepad assignedGamepad; 
    private Rigidbody rb; 
    private bool isGrounded; 
    private bool jumpRequested = false; 
    private Vector2 moveInput; 
    private GameObject heldCherry; 
    private bool isCharging; 
    private GameObject nearbyCherry; 
    public Projectile projectileScript; 
    private Vector2 smoothLookInput; 
    private Vector3 lastLookDir = Vector3.forward; 
    void Start() 
    { 
        if (Gamepad.all.Count > playerIndex) assignedGamepad = Gamepad.all[playerIndex]; 
        rb = GetComponent<Rigidbody>(); 
        projectileScript = GetComponent<Projectile>(); 
        //if (projectileScript != null) projectileScript.SetOwner(this); 
    } 
    private void FixedUpdate() 
    { 
        // --- Movement (Left Stick) ---
        moveInput = assignedGamepad.leftStick.ReadValue(); 
        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, groundCheckDistance, groundLayer); 
        if (isGrounded && moveInput == Vector2.zero) 
        { 
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f); 
        } 

        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y).normalized; 
        Vector3 targetVelocity = move * moveSpeed; 
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z); 
        
        // --- Jump (Button South) ---
        if (jumpRequested) 
        { 
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); 
            jumpRequested = false; 
        } 
    } 
    void Update()
    { 
        if (assignedGamepad == null || rb == null) 
            return; 
        if (isGrounded && assignedGamepad.buttonSouth.wasPressedThisFrame) 
        { 
            jumpRequested = true; 
        } 
        
        // --- Rotation (Right Stick) ---
        Vector2 rawLook = assignedGamepad.rightStick.ReadValue();
        // Smooth input for stability
        smoothLookInput = Vector2.Lerp(smoothLookInput, rawLook, Time.deltaTime * 15f); 
        // Only update direction if stick magnitude is strong enough
        if (smoothLookInput.sqrMagnitude > 0.2f) 
        { 
            lastLookDir = new Vector3(smoothLookInput.x, 0f, smoothLookInput.y).normalized; 
        } 
        Quaternion targetRotation = Quaternion.LookRotation(lastLookDir, Vector3.up); 
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 12f); 
        // --- Pickup / Drop (RT = rightTrigger) ---
        float rtValue = assignedGamepad.rightTrigger.ReadValue(); 
        if (rtValue > 0.1f) // holding RT
        { 
            if (heldCherry == null && nearbyCherry != null) 
            { 
                heldCherry = nearbyCherry; 
                Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>(); 
                if (rbCherry != null) rbCherry.isKinematic = true; 
                heldCherry.transform.SetParent(handHoldPoint); 
                heldCherry.transform.localPosition = Vector3.zero; 
                if (projectileScript != null) projectileScript.PickUpCherry(heldCherry); 
            } 
        } 
        else // released RT
        { if (heldCherry != null && !isCharging) // don't drop while throwing
            { 
                Rigidbody rbCherry = heldCherry.GetComponent<Rigidbody>(); 
                heldCherry.transform.SetParent(null); if (rbCherry != null) rbCherry.isKinematic = false; 
                heldCherry = null; 
            } 
        } 
    } 
    // Pick up cherry
    private void OnTriggerEnter(Collider other) 
    { 
        if (other.CompareTag("Cherry")) 
        { nearbyCherry = other.gameObject; 
        } 
    } 
    private void OnTriggerExit(Collider other) 
    { 
        if (other.CompareTag("Cherry") && other.gameObject == nearbyCherry) 
        { 
            nearbyCherry = null; 
        } 
    } 
    public Gamepad GetAssignedGamepad() { return assignedGamepad; 
    } 
}