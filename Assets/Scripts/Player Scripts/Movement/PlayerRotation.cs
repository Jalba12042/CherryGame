using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    private Rigidbody rb;
    public float rotateSpeed;

    Vector3 rotationLeft;
    Vector3 rotationRight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rotationLeft.Set(0f, -rotateSpeed, 0f);
        rotationRight.Set(0f, rotateSpeed, 0f);

        rotationLeft = -rotationLeft.normalized * -rotateSpeed;
        rotationRight = rotationRight.normalized * rotateSpeed;

        Quaternion deltaRotationLeft = Quaternion.Euler(rotationLeft * Time.fixedDeltaTime);
        Quaternion deltaRotationRight = Quaternion.Euler (rotationRight * Time.fixedDeltaTime);

        if(Input.GetKey(KeyCode.Q))
        {
            rb.MoveRotation(rb.rotation * deltaRotationLeft);
        }
    }
}
