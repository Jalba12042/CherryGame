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
        // list of available items we haven't had yet
        List<ItemData> availableItems = new List<ItemData>();
        foreach (ItemData item in powerUpRegistry)
        {
            if (item.added != true)
            {
                availableItems.Add(item);
            }
        }

        int numButtonsToSetup = Mathf.Min(buttonTexts.Length, availableItems.Count);

        // hashset to remove the possibility of selecting the same item twice
        HashSet<int> chosenIndexes = new HashSet<int>();

        for (int i = 0; i < numButtonsToSetup; i++)
        {
            int randIndex;
            ItemData randItem;

            // picking a random index until we find one we haven't used
            do
            {
                randIndex = Random.Range(0, availableItems.Count);
                randItem = availableItems[randIndex];
            }
            while (chosenIndexes.Contains(randIndex));

            // add the unique index to the set
            chosenIndexes.Add(randIndex);

            // assign the item data to the button
            buttonTexts[i].text = randItem.itemName;
            buttonDescs[i].text = randItem.desc;
        }

        // change text if we run out of unique items
        for (int i = numButtonsToSetup; i < buttonTexts.Length; i++)
        {
            buttonTexts[i].text = "SOLD OUT";
            buttonDescs[i].text = "";
        }
    }
}
