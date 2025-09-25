using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerIndex = 0; // Set this when the player is spawned
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    private Gamepad assignedGamepad;
    private Rigidbody rb;
    private bool isGrounded = true;

    void Start()
    {
        // Assign controller based on player index
        if (Gamepad.all.Count > playerIndex)
        {
            assignedGamepad = Gamepad.all[playerIndex];
        }
        else
        {
            Debug.LogWarning($"No gamepad found for player {playerIndex}");
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("PlayerMovement requires a Rigidbody component.");
        }
    }

    void Update()
    {
        if (assignedGamepad == null || rb == null) return;

        // Movement
        Vector2 moveInput = assignedGamepad.leftStick.ReadValue();
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);

        // Jump
        if (isGrounded && assignedGamepad.buttonSouth.wasPressedThisFrame)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    // Simple ground check using collision
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
