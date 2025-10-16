using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerIndex = 0;
    public float speed = 5f;
    public float jumpForce = 7f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private Gamepad assignedGamepad;
    private bool isGrounded;
    private bool jumpRequested = false;

    void Start()
    {
        if (Gamepad.all.Count > playerIndex)
            assignedGamepad = Gamepad.all[playerIndex];

        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (assignedGamepad == null) return;

        // Check if jump is pressed this frame
        if (isGrounded && assignedGamepad.buttonSouth.wasPressedThisFrame)
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        if (assignedGamepad == null) return;

        // Check if grounded using a raycast
        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, groundCheckDistance, groundLayer);

        // Movement (left stick)
        Vector2 moveInput = assignedGamepad.leftStick.ReadValue();
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        Vector3 targetVelocity = movement * speed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        // Jump
        if (jumpRequested)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequested = false;
        }

        // Apply extra gravity when falling to make jumps feel tighter
        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            rb.AddForce(Vector3.down * 2f, ForceMode.Acceleration); // tweak the 2f
        }

    }

    private void OnDrawGizmosSelected()
    {
        // Visualize ground check ray
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundCheckPoint.position, groundCheckPoint.position + Vector3.down * groundCheckDistance);
        }
    }

    public Gamepad GetAssignedGamepad()
    {
        return assignedGamepad;
    }

}
