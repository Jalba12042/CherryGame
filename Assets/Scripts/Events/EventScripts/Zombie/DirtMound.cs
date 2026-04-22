using UnityEngine;

public class DirtMound : MonoBehaviour
{
    public float riseHeight = 1f;
    public float riseSpeed = 2f;

    private Vector3 startPos;
    private Vector3 targetPos;

    private Zombie zombie;

    private bool isExit = false;

    public enum DirtMode
    {
        Rising,
        Sinking
    }

    private DirtMode mode;

    public void Init(Zombie z)
    {
        zombie = z;
        mode = DirtMode.Rising;
    }

    public void InitExit(Zombie z)
    {
        zombie = z;
        mode = DirtMode.Sinking;
    }

    void Start()
    {
        startPos = transform.position;

        if (mode == DirtMode.Rising)
        {
            targetPos = startPos + Vector3.up * riseHeight;
        }
        else
        {
            targetPos = startPos - Vector3.up * riseHeight;
        }
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
            if (mode == DirtMode.Rising)
            {
                if (zombie != null)
                    zombie.SetDirtFinished();
            }
        }
    }
}