using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    // List of all powerups
    public List<ItemData> powerUpRegistry;

    [SerializeField] private float shopTimerDurationInSecs;
    [SerializeField] private TMP_Text[] buttonTexts;
    [SerializeField] private TMP_Text[] buttonDescs;
    [SerializeField] private TMP_Text timerText;
    private float timer;

    private void Start()
    {
        setupButtons();
        StartCoroutine(StartShopTimer());
    }

    private void Update()
    {
        timerText.text = $"{shopTimerDurationInSecs - (int)timer}";
    }

    // Shop Timer
    private IEnumerator StartShopTimer()
    {
        timer = 0;
        while (timer < shopTimerDurationInSecs)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        timer = shopTimerDurationInSecs;
        RoundManager.Instance.switchRoundScene();
    }
     
    private void setupButtons()
    {
        int[] chosenIndexes = new int[4]; // used to mark which indexes have already been chosen in our registry
        for (int i = 0; i < buttonTexts.Length; i++)
        {
            ItemData randItem;
            int randIndex;
            while (true)
            {
                // pick a random item in the registry
                randIndex = Random.Range(0, powerUpRegistry.Count);
                randItem = powerUpRegistry[randIndex];

                // if the item hasn't been used yet
                if (randItem.added != true)
                {
                    // then we check if the random item isn't apart of the current options
                    bool check = false;
                    for (int j = i; j >= 0; j--)
                    {
                        if (randIndex == chosenIndexes[j])
                            check = true;
                    }
                    if (!check)
                    {
                        chosenIndexes[i] = randIndex;
                        break;
                    }
                }
            }
            
            buttonTexts[i].text = randItem.itemName;
            buttonDescs[i].text = randItem.desc;
        }
    }
}
