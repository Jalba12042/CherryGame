using UnityEngine;

public class Balance : MonoBehaviour
{
    public float uprightForce = 10f;
    public float uprightTorque = 100f;
    public Transform bodyRoot; // assign your spine or torso bone here

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // 1?? Keep upright
        Quaternion current = rb.rotation;
        Quaternion target = Quaternion.Euler(0, current.eulerAngles.y, 0); // stay vertical
        Quaternion toGoal = Quaternion.FromToRotation(current * Vector3.up, Vector3.up);
        Vector3 torque = new Vector3(toGoal.x, 0, toGoal.z) * uprightTorque;
        rb.AddTorque(torque);

        // 2?? Add force to keep the body near its target position (optional)
        if (bodyRoot)
        {
            Vector3 desired = new Vector3(rb.position.x, bodyRoot.position.y, rb.position.z);
            Vector3 correction = desired - rb.position;
            rb.AddForce(correction * uprightForce);
        }
    }
}
