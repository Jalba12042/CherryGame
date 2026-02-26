using UnityEngine;
using System.Collections.Generic;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    [Header("Events")]
    [SerializeField] private List<GameEvent> eventsInRotation;

    [Header("Event curve")]
    [SerializeField] private AnimationCurve eventCurve;

    private float intensity;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public float getIntensityFromCurve()
    {
        return eventCurve.Evaluate(RoundManager.Instance.currRoundProgressNormalized);
    } 

    public GameEvent GetRandomEvent()
    {
        float totalWeight = 0f;

        foreach (var e in eventsInRotation)
            totalWeight += e.weight;

        float randomValue = Random.Range(0, totalWeight);

        foreach (var e in eventsInRotation)
        {
            if (randomValue < e.weight)
                return e;

            randomValue -= e.weight;
        }

        return null;
    }
}
