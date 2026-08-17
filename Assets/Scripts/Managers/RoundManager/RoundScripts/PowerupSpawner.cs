using UnityEngine;

// Spawns a powerup crate inside the "PowerUpSpawnArea" tagged bounds. Call Init() once from a
// Round's setValues(), then SpawnCrate() whenever that Round decides it's time to spawn one.
[System.Serializable]
public class PowerupSpawner
{
    [SerializeField] private GameObject cratePrefab;

    private GameObject powerupSpawnArea;
    private Bounds bounds;

    public void Init()
    {
        powerupSpawnArea = GameObject.FindWithTag("PowerUpSpawnArea");
        if (powerupSpawnArea != null)
        {
            bounds = powerupSpawnArea.GetComponent<Collider>().bounds;
        }
    }

    public void SpawnCrate()
    {
        if (powerupSpawnArea == null || cratePrefab == null) return;

        if (RoundManager.Instance.powerUpsInRotation == null || RoundManager.Instance.powerUpsInRotation.Count == 0)
            return;

        float randX = Random.Range(bounds.min.x, bounds.max.x);
        float randZ = Random.Range(bounds.min.z, bounds.max.z);

        Object.Instantiate(cratePrefab, new Vector3(randX, powerupSpawnArea.transform.position.y, randZ), Quaternion.identity);
    }
}
