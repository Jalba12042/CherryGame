using UnityEngine;

public class DirtMound : MonoBehaviour
{
    public float riseHeight = 1f;
    public float riseSpeed = 2f;

    private Vector3 startPos;
    private Vector3 targetPos;

    private Zombie zombie;

    public void Init(Zombie z)
    {
        zombie = z;
    }

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + Vector3.up * riseHeight;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            riseSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            if (zombie != null)
                zombie.SetDirtFinished();

            enabled = false;
        }
    }
}