using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRotation : MonoBehaviour
{
    private Rigidbody rb;
    public float rotateSpeed = 180f; // degrees per second
    public int playerIndex = 0;

    private Gamepad assignedGamepad;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (Gamepad.all.Count > playerIndex)
            assignedGamepad = Gamepad.all[playerIndex];
    }

    void FixedUpdate()
    {
        if (assignedGamepad == null) return;

        // Read right stick input
        Vector2 lookInput = assignedGamepad.rightStick.ReadValue();

        // Only rotate if input is significant
        if (lookInput.sqrMagnitude > 0.1f)
        {
            // Convert stick direction to a rotation direction
            Vector3 lookDirection = new Vector3(lookInput.x, 0f, lookInput.y);

            // Smoothly rotate toward that direction
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime));
        }
    }
}
