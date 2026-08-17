using UnityEngine;

public class PlayerShadow : MonoBehaviour
{
    [SerializeField] private GameObject shadow;
    [SerializeField] private LayerMask shadowOccluderLayer;

    private Camera mainCamera;
    private Collider playerCollider;

    private void Start()
    {
        shadow.SetActive(false);

        mainCamera = Camera.main;
        playerCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        Vector3 directionToPlayer = playerCollider.bounds.center - mainCamera.transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        RaycastHit hit;

        if (Physics.Raycast(
            mainCamera.transform.position,
            directionToPlayer.normalized,
            out hit,
            distanceToPlayer,
            shadowOccluderLayer))
        {
            shadow.SetActive(true);
        }
        else
        {
            shadow.SetActive(false);
        }
    }
}