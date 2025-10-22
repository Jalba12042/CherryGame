using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ShopManager : MonoBehaviour
{
    // List of all powerups
    public List<ItemData> powerUpRegistry;

    [SerializeField] private float shopTimerDurationInSecs;
    [SerializeField] private TMP_Text[] buttonTexts;
    [SerializeField] private TMP_Text[] buttonDescs;
    [SerializeField] private TMP_Text timerText;
    private float timer;
    public Button[] shopButtons;

    [Header("Highlight Images (per player per button)")]
    public Image[,] playerHighlights = new Image[4, 4]; // [playerIndex, buttonIndex]

    private int[] currentIndexes = new int[4];
    private bool[] canMove = new bool[4];
    private int[] buttonVotes;

    private Color[] playerColors = { Color.blue, Color.red, Color.green, Color.yellow };

    private void Start()
    {
        setupButtons();

        int playerCount = Mathf.Min(GameManager.Instance.playerCount, Gamepad.all.Count);
        buttonVotes = new int[shopButtons.Length];

        for (int i = 0; i < 4; i++)
        {
            currentIndexes[i] = 0;
            canMove[i] = true;
        }

        SetupHighlights();
        HighlightButtons();

        StartCoroutine(StartShopTimer());
    }

    private void Update()
    {
        timerText.text = $"{shopTimerDurationInSecs - (int)timer}";
        HandleControllerInput();
    }

    private void HandleControllerInput()
    {
        int playerCount = Mathf.Min(GameManager.Instance.playerCount, Gamepad.all.Count);

        for (int i = 0; i < playerCount; i++)
        {
            var gamepad = Gamepad.all[i];
            Vector2 move = gamepad.leftStick.ReadValue();

            if (canMove[i])
            {
                if (move.y > 0.5f)
                {
                    currentIndexes[i] = Mathf.Max(0, currentIndexes[i] - 1);
                    canMove[i] = false;
                    HighlightButtons();
                }
                else if (move.y < -0.5f)
                {
                    currentIndexes[i] = Mathf.Min(shopButtons.Length - 1, currentIndexes[i] + 1);
                    canMove[i] = false;
                    HighlightButtons();
                }
            }

            if (Mathf.Abs(move.y) < 0.2f)
                canMove[i] = true;

            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                int chosenButton = currentIndexes[i];
                buttonVotes[chosenButton]++;
                Debug.Log($"Player {i + 1} (Color {playerColors[i]}) voted for button {chosenButton + 1}. Total votes: {buttonVotes[chosenButton]}");
            }
        }
    }

    private void HighlightButtons()
    {
        int playerCount = Mathf.Min(GameManager.Instance.playerCount, Gamepad.all.Count);

        for (int p = 0; p < playerCount; p++)
        {
            for (int b = 0; b < shopButtons.Length; b++)
            {
                if (playerHighlights[p, b] != null)
                {
                    playerHighlights[p, b].enabled = (b == currentIndexes[p]);
                }
            }
        }
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

        for (int i = 0; i < buttonVotes.Length; i++)
        {
            Debug.Log($"Button {i + 1} got {buttonVotes[i]} votes");
        }

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

    private void SetupHighlights()
    {
        // Auto-create small highlight images under each button for each player
        int playerCount = Mathf.Min(GameManager.Instance.playerCount, 4);

        for (int p = 0; p < playerCount; p++)
        {
            for (int b = 0; b < shopButtons.Length; b++)
            {
                GameObject highlightObj = new GameObject($"Player{p + 1}_Highlight_Button{b + 1}");
                highlightObj.transform.SetParent(shopButtons[b].transform, false);

                Image img = highlightObj.AddComponent<Image>();
                img.color = playerColors[p];
                img.enabled = false;

                // position/scale highlight (slightly smaller than button)
                RectTransform rt = highlightObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.05f, 0.05f);
                rt.anchorMax = new Vector2(0.95f, 0.95f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;

                playerHighlights[p, b] = img;
            }
        }
    }
}
