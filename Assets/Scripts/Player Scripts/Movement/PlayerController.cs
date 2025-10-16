using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerIndex = 0;
    public float speed;
    private Rigidbody rb;
    private Gamepad assignedGamepad;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Gamepad.all.Count > playerIndex) assignedGamepad = Gamepad.all[playerIndex];
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (assignedGamepad == null) return;

        // Get left stick input
        Vector2 moveInput = assignedGamepad.leftStick.ReadValue();

        // Convert to 3D movement vector
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);

        // Apply movement velocity
        Vector3 targetVelocity = movement * speed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }
}
