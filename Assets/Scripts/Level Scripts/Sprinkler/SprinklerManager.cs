using System.Collections;
using UnityEngine;

public class SprinklerManager : MonoBehaviour
{
    [Header("Spawn Volume")]
    public BoxCollider[] spawnAreas;

    [Header("Sprinkler")]
    public GameObject sprinklerPrefab;

    [Header("Timing")]
    public float timeBetweenEvents = 10f;
    public float riseHeight = 2f;

    private GameObject currentSprinkler;
    private Coroutine sprinklerRoutine;


    public void StartSprinklers()
    {
        if (sprinklerRoutine == null)
            sprinklerRoutine = StartCoroutine(SprinklerLoop());
    }

    public void StopSprinklers()
    {
        if (sprinklerRoutine != null)
        {
            StopCoroutine(sprinklerRoutine);
            sprinklerRoutine = null;
        }
    }

    private IEnumerator SprinklerLoop()
    {
        while (true)
        {
            // random delay before spawn
            float delay = Random.Range(timeBetweenEvents, timeBetweenEvents + 5f);
            yield return new WaitForSeconds(delay);

            yield return StartCoroutine(SpawnSprinklerEvent());

            // extra cooldown AFTER it finishes
            yield return new WaitForSeconds(3f);
        }
    }

    private Vector3 GetRandomPointInBox()
    {
        if (spawnAreas == null || spawnAreas.Length == 0)
        {
            Debug.LogWarning("No spawn areas assigned!");
            return Vector3.zero;
        }

        // Pick a random spawn area
        BoxCollider chosenArea = spawnAreas[Random.Range(0, spawnAreas.Length)];

        Vector3 center = chosenArea.bounds.center;
        Vector3 size = chosenArea.bounds.size;

        float x = Random.Range(-size.x / 2f, size.x / 2f);
        float z = Random.Range(-size.z / 2f, size.z / 2f);

        return new Vector3(center.x + x, center.y, center.z + z);
    }

    private IEnumerator SpawnSprinklerEvent()
    {
        Vector3 spawnPos = GetRandomPointInBox();

        currentSprinkler = Instantiate(sprinklerPrefab, spawnPos, Quaternion.identity);

        Vector3 hiddenPos = spawnPos - Vector3.up * riseHeight;

        currentSprinkler.transform.position = hiddenPos;

        // rise
        while (Vector3.Distance(currentSprinkler.transform.position, spawnPos) > 0.01f)
        {
            currentSprinkler.transform.position = Vector3.MoveTowards(
                currentSprinkler.transform.position,
                spawnPos,
                Time.deltaTime * 2f
            );
            yield return null;
        }

        // activate effects
        currentSprinkler.GetComponent<SprinklerBehaviour>()?.Activate();

        yield return new WaitForSeconds(5f);

        // go back down
        while (Vector3.Distance(currentSprinkler.transform.position, hiddenPos) > 0.01f)
        {
            currentSprinkler.transform.position = Vector3.MoveTowards(
                currentSprinkler.transform.position,
                hiddenPos,
                Time.deltaTime * 2f
            );
            yield return null;
        }

        Destroy(currentSprinkler);
    }
}