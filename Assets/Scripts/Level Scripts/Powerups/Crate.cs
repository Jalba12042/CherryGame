using UnityEngine;

public class Crate : MonoBehaviour
{
    private bool hasSpawned = false;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (hasSpawned) return;
            else
            {
                hasSpawned = true;
                GetComponent<MeshRenderer>().enabled = false;
                GetComponent<Collider>().enabled = false;

                Instantiate(RoundManager.Instance.powerUpsInRotation[Random.Range(0, RoundManager.Instance.powerUpsInRotation.Count)], transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
}
