using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Linearc : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public int resolution = 20;
    public float arcHeight = 5f; // ← increase this for a higher arch

    private LineRenderer lr;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = resolution;
    }

    void Update()
    {
        DrawArc();
    }

    void DrawArc()
    {
        Vector3 p0 = startPoint.position;
        Vector3 p1 = endPoint.position;

        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1);
            Vector3 pos = Vector3.Lerp(p0, p1, t);

            // Add the arc height in the middle of the curve
            pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

            lr.SetPosition(i, pos);
        }
    }
}
