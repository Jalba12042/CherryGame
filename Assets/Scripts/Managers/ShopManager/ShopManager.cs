using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    // List of all powerups
    public List<ItemData> powerUpRegistry;

    [SerializeField] private float shopTimerDurationInSecs;
    [SerializeField] private TMP_Text timerText;
    private float timer;

    private void Start()
    {
        StartCoroutine(StartShopTimer());
    }

    private void Update()
    {
        timerText.text = $"{shopTimerDurationInSecs - (int)timer}";
    }

    private IEnumerator StartShopTimer()
    {
        timer = 0;
        while (timer < shopTimerDurationInSecs)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        timer = shopTimerDurationInSecs;
    }
}
