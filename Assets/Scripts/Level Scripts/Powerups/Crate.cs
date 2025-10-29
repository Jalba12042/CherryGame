using UnityEngine;

public class Crate : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Collider>().enabled = false;

            Instantiate(RoundManager.Instance.powerUpsInRotation[Random.Range(0, RoundManager.Instance.powerUpsInRotation.Count)], transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
