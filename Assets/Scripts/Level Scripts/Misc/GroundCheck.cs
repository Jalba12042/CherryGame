using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool isGrounded;

    [Header("Ground Check Info")]
    public float groundCheckDistance;
    public Vector3 origin;
    [SerializeField] private LayerMask groundLayer;
    public Vector3 groundCheckOffset = new Vector3(0f, 0.1f, 0f);
    private void FixedUpdate()
    {
        origin = transform.position + groundCheckOffset;
        isGrounded = Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
    }
}
