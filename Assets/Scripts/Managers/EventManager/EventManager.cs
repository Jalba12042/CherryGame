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

    [Header("Animated UI Screens")]
    public GameObject meteorAnimatedUI;
    public GameObject cherryAnimatedUI;
    public GameObject ufoAnimatedUI;
    public GameObject zombieAnimatedUI;
    public GameObject mirrorAnimatedUI;

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

        // Clean up UI
        if (meteorAnimatedUI != null) meteorAnimatedUI.SetActive(false);
        if (cherryAnimatedUI != null) cherryAnimatedUI.SetActive(false);
        if (ufoAnimatedUI != null) ufoAnimatedUI.SetActive(false);
        if (zombieAnimatedUI != null) zombieAnimatedUI.SetActive(false);
        if (mirrorAnimatedUI != null) mirrorAnimatedUI.SetActive(false);
        if (eventTextObj != null) eventTextObj.SetActive(false);
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
        GameObject activeUI = null;

        if (triggeredEvent.eventName == "Meteor Shower!") activeUI = meteorAnimatedUI;
        else if (triggeredEvent.eventName == "Cherry Fever!") activeUI = cherryAnimatedUI;
        else if (triggeredEvent.eventName == "Alien Invasion!") activeUI = ufoAnimatedUI;
        else if (triggeredEvent.eventName == "Zombie Apocalypse!") activeUI = zombieAnimatedUI;
        else if (triggeredEvent.eventName == "Magic Mirror!") activeUI = mirrorAnimatedUI;

        if (activeUI != null)
        {
            activeUI.SetActive(true);
            yield return new WaitForSeconds(3f);
            activeUI.SetActive(false);
        }
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