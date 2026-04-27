using UnityEngine;

public class PowerUpFloat : MonoBehaviour
{
    [Header("Float Settings")]
    public float floatSpeed = 1f;     
    public float floatHeight = 0.1f;   

    [Header("Spin Settings")]
    public float spinSpeed = 50f;      

    [Header("Sway Settings")]
    public float swaySpeed = 1f;       
    public float swayAmount = 10f;     

    private Vector3 startPos;
    private bool isHeld = false;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {

        if (isHeld) return;

        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = startPos + new Vector3(0f, yOffset, 0f);

        
        float spin = Time.time * spinSpeed;

        //spooky
        float sway = Mathf.Sin(Time.time * swaySpeed) * swayAmount;

      
        transform.rotation = Quaternion.Euler(sway, spin, 0f);
    }

    public void SetHeld(bool held)
    {
        isHeld = held;
    }
}