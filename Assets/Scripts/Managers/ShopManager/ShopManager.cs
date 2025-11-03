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
    private int amtOfButtons;
    private int[] playerVotes;


    [Header("Highlight Images (per player per button)")]
    public Image[,] playerHighlights = new Image[4, 4]; // [playerIndex, buttonIndex]

    private int[] currentIndexes = new int[4];
    private bool[] canMove = new bool[4];
    private int[] buttonVotes;
    private List<ItemData> powerUps;
    private ItemData addedPowerUp;

    private Color[] playerColors = { Color.blue, Color.red, Color.green, Color.yellow };

    private int playerCount;

    private void Start()
    {
        setupButtons();

        playerCount = Mathf.Min(GameManager.Instance.playerCount, Gamepad.all.Count);
        buttonVotes = new int[amtOfButtons];
        for (int i = 0; i < playerCount; i++)
        {
            currentIndexes[i] = 0;
            canMove[i] = true;
        }

        playerVotes = new int[4];
        for (int i = 0; i < 4; i++)
        {
            playerVotes[i] = -1; // start with no votes
        }

        SetupHighlights();
        HighlightButtons();

        StartCoroutine(StartShopTimer());
    }

    private void Update()
    {
        timerText.text = $"{shopTimerDurationInSecs - (int)timer}";
        timerText.color = timer >= shopTimerDurationInSecs - 3f ? Color.red : Color.white;
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
                // UP
                if (move.y > 0.5f)
                {
                    if (currentIndexes[i] - 2 >= 0)
                        currentIndexes[i] -= 2; // Move up one row
                    canMove[i] = false;
                    HighlightButtons();
                }
                // DOWN
                else if (move.y < -0.5f)
                {
                    if (currentIndexes[i] + 2 < shopButtons.Length)
                        currentIndexes[i] += 2; // Move down one row
                    canMove[i] = false;
                    HighlightButtons();
                }
                // LEFT
                else if (move.x < -0.5f)
                {
                    if (currentIndexes[i] % 2 != 0)
                        currentIndexes[i] -= 1; // Move left
                    canMove[i] = false;
                    HighlightButtons();
                }
                // RIGHT
                else if (move.x > 0.5f)
                {
                    if (currentIndexes[i] % 2 == 0 && currentIndexes[i] + 1 < shopButtons.Length)
                        currentIndexes[i] += 1; // Move right
                    canMove[i] = false;
                    HighlightButtons();
                }
            }

            // Reset movement gating (prevents rapid flicking)
            if (Mathf.Abs(move.y) < 0.2f && Mathf.Abs(move.x) < 0.2f)
                canMove[i] = true;


            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                int chosenButton = currentIndexes[i];

                // Remove old vote if there was one
                int oldVote = playerVotes[i];
                if (oldVote != -1 && chosenButton < amtOfButtons)
                {
                    buttonVotes[oldVote]--;
                }

                // Add new vote
                if (chosenButton < amtOfButtons)
                {
                    buttonVotes[chosenButton]++;
                    playerVotes[i] = chosenButton;
                    Debug.Log($"Player {i + 1} (Color {playerColors[i]}) voted for button {chosenButton + 1}. Total votes: {buttonVotes[chosenButton]}");
                }                
            }

        }
    }

    private void HighlightButtons()
    {
        int playerCount = Mathf.Min(GameManager.Instance.playerCount, Gamepad.all.Count);

        // First, disable ALL highlights for every player and every button
        for (int p = 0; p < playerCount; p++)
        {
            for (int b = 0; b < shopButtons.Length; b++)
            {
                if (playerHighlights[p, b] != null)
                    playerHighlights[p, b].enabled = false;
            }
        }

        // Then, enable ONLY the ones the players are currently on
        for (int p = 0; p < playerCount; p++)
        {
            int current = Mathf.Clamp(currentIndexes[p], 0, shopButtons.Length - 1);
            if (playerHighlights[p, current] != null)
                playerHighlights[p, current].enabled = true;

        }
    }



    // Shop Timer
    private IEnumerator StartShopTimer()
    {
        timer = 0;
        while (timer < shopTimerDurationInSecs)
        {
            timer += Time.deltaTime;

            // change timer to 3 after everyone has voted
            int votesCounted = 0;
            for (int i = 0; i < playerCount; i++)
            {
                if (playerVotes[i] != -1)
                {
                    votesCounted++;
                }
            }

            if (votesCounted == playerCount)
            {
                timer = Mathf.Max(timer, shopTimerDurationInSecs - 3f);
            }
            yield return null;
        }
        timer = shopTimerDurationInSecs;

        int currentHighestVal = -1;
        List<int> winners = new List<int>();

        for (int i = 0; i < buttonVotes.Length; i++)
        {
            Debug.Log($"Button {i + 1} got {buttonVotes[i]} votes");

            // check for a winner(s)
            if (buttonVotes[i] > currentHighestVal)
            {
                winners.Clear();
                winners.Add(i);
                currentHighestVal = buttonVotes[i];
            }
            else if (buttonVotes[i] == currentHighestVal)
            {
                winners.Add(i);
                currentHighestVal = buttonVotes[i];
            }
        }

        int winnerIndex = winners[0];

        // if there's a tie
        if (winners.Count > 1)
        {
            int randIndex = Random.Range(0, winners.Count);
            winnerIndex = winners[randIndex];
        }

        addedPowerUp = powerUps[winnerIndex];
/*        //addedPowerUp.added = true;

        for (int i = 0; i < powerUpRegistry.Count; i++)
        {
            if (powerUpRegistry[i] == addedPowerUp)
            {
                //powerUpRegistry[i].added = true;
                break;
            }
        }*/

        RoundManager.Instance.powerUpsInRotation.Add(addedPowerUp.powerup);
        RoundManager.Instance.switchRoundScene();
    }

    private void setupButtons()
    {
        powerUps = new List<ItemData>();
        amtOfButtons = shopButtons.Length;

        // list of available items we haven't had yet
        List<ItemData> availableItems = new List<ItemData>();
        foreach (ItemData item in powerUpRegistry)
        {
            if (!RoundManager.Instance.powerUpsInRotation.Contains(item.powerup))
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

            powerUps.Add(randItem);
        }

        // change text if we run out of unique items
        for (int i = numButtonsToSetup; i < buttonTexts.Length; i++)
        {
            buttonTexts[i].text = "SOLD OUT";
            buttonDescs[i].text = "";
            amtOfButtons--;
        }
    }

    private void SetupHighlights()
    {
        int playerCount = Mathf.Min(GameManager.Instance.playerCount, Gamepad.all.Count);

        for (int p = 0; p < playerCount; p++)
        {
            for (int b = 0; b < shopButtons.Length; b++)
            {
                // Create highlight overlay as a child of the button
                GameObject highlightObj = new GameObject($"Player{p + 1}_Highlight_Button{b + 1}");
                highlightObj.transform.SetParent(shopButtons[b].transform, false);

                Image img = highlightObj.AddComponent<Image>();
                img.color = playerColors[p]; // blue, red, green, yellow
                img.raycastTarget = false; 

                RectTransform rt = highlightObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 1);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                // Make it slightly transparent
                Color c = img.color;
                c.a = 0.25f; // Adjust transparency here
                img.color = c;

                img.enabled = false; // start off
                playerHighlights[p, b] = img;
            }
        }
    }


}
