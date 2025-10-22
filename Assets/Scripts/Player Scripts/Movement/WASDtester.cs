using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WASDtester : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpSpeed = 6f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    [HideInInspector]
    public bool IsSlowed = false; // Required by SprinklerInteraction

    private Rigidbody rb;
    private Vector2 input;
    private bool tryJump;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        // Gather input (fixed camera)
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
            tryJump = true;
    }

    void FixedUpdate()
    {
        // Ground detection
        bool isGrounded = groundCheck != null &&
                          Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Move player
        Vector3 planar = new Vector3(input.x, 0f, input.y).normalized * moveSpeed;
        Vector3 currentVel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(planar.x, currentVel.y, planar.z);

        // Jump
        if (tryJump && isGrounded)
        {
            Vector3 v = rb.linearVelocity;
            v.y = jumpSpeed;
            rb.linearVelocity = v;
        }

        tryJump = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}
