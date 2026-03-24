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

    [Header("Text UI (Restored for RoundManager)")]
    public GameObject eventTextObj;
    public TMP_Text eventText;

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
        // When the EventManager travels to a new scene, it will automatically search for your images!
        if (meteorAnimatedUI == null)
        {
            GameObject foundObj = GameObject.Find("MeteorImage");
            if (foundObj != null) { meteorAnimatedUI = foundObj; meteorAnimatedUI.SetActive(false); }
        }
        if (cherryAnimatedUI == null)
        {
            GameObject foundObj = GameObject.Find("CherryImage");
            if (foundObj != null) { cherryAnimatedUI = foundObj; cherryAnimatedUI.SetActive(false); }
        }
        if (ufoAnimatedUI == null)
        {
            GameObject foundObj = GameObject.Find("UFOImage");
            if (foundObj != null) { ufoAnimatedUI = foundObj; ufoAnimatedUI.SetActive(false); }
        }

        // Keeps RoundManager happy
        if (eventTextObj != null && eventText == null)
        {
            eventText = eventTextObj.GetComponent<TMP_Text>();
            eventTextObj.SetActive(false);
        }

        if (!RoundManager.Instance.currRoundActive && currEventRoutine != null)
        {
            StopCoroutine(currEventRoutine);
            if (UIRoutine != null) StopCoroutine(UIRoutine);
            currEventRoutine = null;

            if (currEvent != null)
                currEvent.isRunning = false;

            currEvent = null;
            eventRunning = false;

            // Failsafe: Turn off all UI animations if the round ends abruptly
            if (meteorAnimatedUI != null) meteorAnimatedUI.SetActive(false);
            if (cherryAnimatedUI != null) cherryAnimatedUI.SetActive(false);
            if (ufoAnimatedUI != null) ufoAnimatedUI.SetActive(false);
            if (eventTextObj != null) eventTextObj.SetActive(false);
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

    public IEnumerator EventTimer()
    {
        while (RoundManager.Instance.currRoundActive)
        {
            yield return new WaitForSeconds(2f);
            if (Random.value < getIntensityFromCurve() && currEvent == null && !onCooldown)
            {
                Debug.Log("started");
                currEvent = GetRandomEvent();

                UIRoutine = StartCoroutine(UITimer(currEvent));

                currEventRoutine = StartCoroutine(currEvent.Trigger());
                eventRunning = true;
            }
        }
    }

    public IEnumerator UITimer(GameEvent triggeredEvent)
    {
        GameObject activeUI = null;

        if (triggeredEvent.eventName == "Meteor Shower!")
        {
            activeUI = meteorAnimatedUI;
        }
        else if (triggeredEvent.eventName == "Cherry Fever!") // FIXED: Now exactly matches the error log!
        {
            activeUI = cherryAnimatedUI;
        }
        else if (triggeredEvent.eventName == "Alien Invasion!")
        {
            activeUI = ufoAnimatedUI;
        }

        if (activeUI != null)
        {
            activeUI.SetActive(true);

            yield return new WaitForSeconds(3f);

            activeUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Could not find a matching UI for event: " + triggeredEvent.eventName);
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