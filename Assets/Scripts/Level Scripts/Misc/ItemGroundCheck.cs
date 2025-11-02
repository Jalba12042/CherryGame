using UnityEngine;

public class ItemGroundCheck : MonoBehaviour
{
    public bool isGrounded;

    [Header("Ground Check Info")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector3 groundCheckOffset = new Vector3(0f, 0.1f, 0f);
    private void FixedUpdate()
    {
        Vector3 origin = transform.position + groundCheckOffset;
        isGrounded = Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 origin = transform.position + groundCheckOffset;
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
    }
}
