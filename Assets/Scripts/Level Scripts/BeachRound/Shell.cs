using UnityEngine;
using System.Collections;

public class Shell : LevelPickup
{
    protected override void Awake()
    {
        base.Awake();
        useProjectileThrow = true;
    }

    protected override void Update()
    {
        base.Update();
    }

}
