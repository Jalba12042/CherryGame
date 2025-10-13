using UnityEngine;

public class RagdollPlayer : MonoBehaviour
{
    public Rigidbody hips;          // drag your hips bone here in Inspector
    public float moveForce = 300f;
    public float jumpForce = 250f;
    public float uprightTorque = 100f;

    private bool isGrounded;

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool jump = Input.GetKeyDown(KeyCode.Space);

        // Move the hips using physics
        Vector3 move = new Vector3(h, 0, v).normalized;
        if (move.magnitude > 0.1f)
        {
            hips.AddForce(move * moveForce * Time.deltaTime);
        }

        // Jump
        if (jump && isGrounded)
        {
            hips.AddForce(Vector3.up * jumpForce);
            isGrounded = false;
        }

        // Stay upright
        Vector3 upDir = hips.transform.up;
        Vector3 uprightError = Vector3.Cross(upDir, Vector3.up);
        hips.AddTorque(uprightError * uprightTorque * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only consider collisions with “ground” layer
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }
}