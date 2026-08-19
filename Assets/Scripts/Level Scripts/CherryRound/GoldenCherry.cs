using UnityEngine;

// Behaves exactly like a normal Cherry, just worth more points.
public class GoldenCherry : Cherry
{
    protected override void Awake()
    {
        base.Awake();
        pointValue = 3;
    }
}
