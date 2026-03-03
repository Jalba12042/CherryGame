using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    [Header("Events")]
    [SerializeField] private List<GameEvent> eventsInRotation;
    public GameEvent currEvent;
    public Coroutine currEventRoutine;

    [Header("Event curve")]
    [SerializeField] private AnimationCurve eventCurve;

    public bool eventRunning;
    public bool onCooldown;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            eventRunning = false;
            onCooldown = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!RoundManager.Instance.currRoundActive && currEventRoutine != null)
        {
            StopCoroutine(currEventRoutine);
            currEventRoutine = null;

            if (currEvent != null)
                currEvent.isRunning = false;

            currEvent = null;
            eventRunning = false;
        }

        if (currEvent != null)
        {
            if (eventRunning && !currEvent.isRunning)
            {
                Debug.Log(currEvent.name + " has ended");
                StartCoroutine(CooldownTimer(currEvent.cooldown));
                eventRunning = false;
                currEvent = null;
            }
        }
    }

    public float getIntensityFromCurve()
    {
        return eventCurve.Evaluate(RoundManager.Instance.currRoundProgressNormalized);
    } 

    // returns a random event, events with more weight are more likely to show up
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

    // starts a new event every two seconds depending on our event graph, the larger the value returned by the intensity the more likely an event is to occur in that two seconds
    public IEnumerator EventTimer()
    {
        while (RoundManager.Instance.currRoundActive)
        {
            yield return new WaitForSeconds(2f);
            if (Random.value < getIntensityFromCurve() && currEvent == null && !onCooldown)
            {
                Debug.Log("started");
                currEvent = GetRandomEvent();
                currEventRoutine = StartCoroutine(currEvent.Trigger());
                eventRunning = true;
            }
        }
    }

    public IEnumerator CooldownTimer(float cooldown)
    {
        onCooldown = true;

        float endTime = Time.time + cooldown;

        while (Time.time < endTime && RoundManager.Instance.currRoundActive)
        {
            yield return null;
        }

        onCooldown = false;
    }
}
