using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    [Header("Events")]
    [SerializeField] private List<GameEvent> eventsInRotation;
    public GameEvent currEvent;
    public Coroutine currEventRoutine;
    public Coroutine UIRoutine;

    [Header("Event curve")]
    [SerializeField] private AnimationCurve eventCurve;

    [Header("Event UI Transform")]
    [SerializeField] private Transform eventUIPoint;
    [SerializeField] private string eventUITag;

    [Header("Text UI")]
    public GameObject eventTextObj;
    public TMP_Text eventText;

    public bool eventRunning;
    public bool onCooldown;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // The EventManager must be immortal to travel between scenes
            DontDestroyOnLoad(gameObject);
            eventRunning = false;
            onCooldown = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- THIS MUST BE PUBLIC FOR GAMEMANAGER/PAUSEMANAGER TO ACCESS IT ---
    public void SoftReset()
    {
        StopAllCoroutines();
        currEventRoutine = null;
        UIRoutine = null;

        if (currEvent != null) currEvent.isRunning = false;
        currEvent = null;

        eventRunning = false;
        onCooldown = false;
    }

    private void Update()
    {
        // Safety check for RoundManager
        if (RoundManager.Instance != null && !RoundManager.Instance.currRoundActive && currEventRoutine != null)
        {
            SoftReset();
        }

        if (currEvent != null)
        {
            if (eventRunning && !currEvent.isRunning)
            {
                StartCoroutine(CooldownTimer(currEvent.cooldown));
                eventRunning = false;
                currEvent = null;
            }
        }
    }

    public float getIntensityFromCurve()
    {
        if (RoundManager.Instance == null) return 0f;
        return eventCurve.Evaluate(RoundManager.Instance.currRoundProgressNormalized);
    }

    public GameEvent GetRandomEvent()
    {
        List<GameEvent> excluded = RoundManager.Instance.currRound?.excludedEvents;
        float totalWeight = 0f;
        foreach (var e in eventsInRotation)
        {
            if (excluded != null && excluded.Contains(e)) continue;
            totalWeight += e.weight;
        }

        if (totalWeight <= 0f) return null;
        float randomValue = Random.Range(0, totalWeight);

        foreach (var e in eventsInRotation)
        {
            if (excluded != null && excluded.Contains(e)) continue;
            if (randomValue < e.weight) return e;
            randomValue -= e.weight;
        }
        return null;
    }

    public IEnumerator EventTimer()
    {
        while (RoundManager.Instance != null && RoundManager.Instance.currRoundActive)
        {
            yield return new WaitForSeconds(2f);
            if (Random.value < getIntensityFromCurve() && currEvent == null && !onCooldown)
            {
                currEvent = GetRandomEvent();
                if (currEvent != null)
                {
                    UIRoutine = StartCoroutine(UITimer(currEvent));
                    currEventRoutine = StartCoroutine(currEvent.Trigger());
                    eventRunning = true;
                }
            }
        }
    }

    public IEnumerator UITimer(GameEvent triggeredEvent)
    {
        GameObject eventUIPointObj = GameObject.FindWithTag(eventUITag);
        if (eventUIPointObj == null)
        {
            Debug.LogWarning($"[EventManager] No GameObject tagged '{eventUITag}' found for event UI.");
            yield break;
        }
        eventUIPoint = eventUIPointObj.transform;

        GameObject activeUI = triggeredEvent.animatedUI;
        if (activeUI == null) yield break;

        // parent under eventUIPoint (a child of the Canvas) so the UI's CanvasRenderer actually renders
        GameObject currActiveUI = Instantiate(activeUI, eventUIPoint);
        currActiveUI.SetActive(true);
        yield return new WaitForSeconds(3f);
        Destroy(currActiveUI);
    }

    public IEnumerator CooldownTimer(float cooldown)
    {
        onCooldown = true;
        float endTime = Time.time + cooldown;
        while (Time.time < endTime && RoundManager.Instance != null && RoundManager.Instance.currRoundActive)
        {
            yield return null;
        }
        onCooldown = false;
    }
}