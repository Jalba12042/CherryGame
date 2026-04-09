using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private VoteImageManager vim;

    public List<ItemData> powerUpRegistry;
    [SerializeField] private float shopTimerDurationInSecs;

    [Header("UI Elements (Images Only!)")]
    [SerializeField] private Image[] itemSlots;
    [SerializeField] private GameObject[] numberIconObjects;
    [SerializeField] private Sprite soldOutSprite;

    // NEW: Changed this to an Array so you can put all 4 text boxes in here!
    [SerializeField] private TMP_Text[] stickyNoteTexts;

    [SerializeField] private TMP_Text timerText;

    private float timer;
    private int amtOfItems;
    private int[] playerVotes;

    [Header("Highlight Images")]
    public Image[,] playerHighlights = new Image[4, 4];

    private int[] currentIndexes = new int[4];
    private bool[] canMove = new bool[4];
    private int[] itemVotes;
    private List<ItemData> powerUps;
    private ItemData addedPowerUp;

    private Color[] playerColors = { Color.blue, Color.red, Color.green, Color.yellow };
    private int playerCount;

    private void Start()
    {
        setupItems();

        playerCount = Mathf.Min(GameManager.Instance.playerCount, Gamepad.all.Count);
        itemVotes = new int[amtOfItems];
        for (int i = 0; i < playerCount; i++)
        {
            currentIndexes[i] = 0;
            canMove[i] = true;
        }

        playerVotes = new int[4];
        for (int i = 0; i < 4; i++)
        {
            playerVotes[i] = -1;
        }

        SetupHighlights();
        HighlightItems();
        UpdateStickyNote();

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
        int playerCount = GameManager.Instance.playerCount;

        // Quick Keyboard testing for Player 1
        if (Keyboard.current != null)
        {
            int kVote = -1;
            if (Keyboard.current.digit1Key.wasPressedThisFrame) kVote = 0;
            if (Keyboard.current.digit2Key.wasPressedThisFrame) kVote = 1;
            if (Keyboard.current.digit3Key.wasPressedThisFrame) kVote = 2;
            if (Keyboard.current.digit4Key.wasPressedThisFrame) kVote = 3;

            if (kVote != -1 && kVote < amtOfItems)
            {
                if (playerVotes[0] != -1) itemVotes[playerVotes[0]]--;
                itemVotes[kVote]++;
                playerVotes[0] = kVote;
                UpdateStickyNote();
            }
        }

        // Loop through all active players
        for (int i = 0; i < playerCount; i++)
        {
            int controllerIndex = GameManager.Instance.controllerAssignments[i];

            // Skip if controller is unassigned or disconnected
            if (controllerIndex < 0 || controllerIndex >= Gamepad.all.Count)
                continue;

            var gamepad = Gamepad.all[controllerIndex];
            Vector2 move = gamepad.leftStick.ReadValue();

            // Movement
            if (canMove[i])
            {
                if (move.y > 0.5f)
                {
                    if (currentIndexes[i] - 2 >= 0) currentIndexes[i] -= 2;
                    canMove[i] = false; HighlightItems();
                }
                else if (move.y < -0.5f)
                {
                    if (currentIndexes[i] + 2 < itemSlots.Length) currentIndexes[i] += 2;
                    canMove[i] = false; HighlightItems();
                }
                else if (move.x < -0.5f)
                {
                    if (currentIndexes[i] % 2 != 0) currentIndexes[i] -= 1;
                    canMove[i] = false; HighlightItems();
                }
                else if (move.x > 0.5f)
                {
                    if (currentIndexes[i] % 2 == 0 && currentIndexes[i] + 1 < itemSlots.Length) currentIndexes[i] += 1;
                    canMove[i] = false; HighlightItems();
                }
            }

            // Reset movement gating
            if (Mathf.Abs(move.y) < 0.2f && Mathf.Abs(move.x) < 0.2f) canMove[i] = true;

            // Selection / voting
            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                int chosenItem = currentIndexes[i];

                int oldVote = playerVotes[i];
                if (oldVote != -1 && chosenItem < amtOfItems)
                {
                    itemVotes[oldVote]--;
                }

                if (chosenItem < amtOfItems)
                {
                    itemVotes[chosenItem]++;
                    playerVotes[i] = chosenItem;
                }

                vim.changeVote(i, chosenItem);
                UpdateStickyNote();
            }
        }
    }

    // NEW: Updated to talk to all 4 of your separate text boxes
    private void UpdateStickyNote()
    {
        for (int i = 0; i < playerCount; i++)
        {
            // Check if you actually put a text box in this slot in the Inspector
            if (i < stickyNoteTexts.Length && stickyNoteTexts[i] != null)
            {
                if (playerVotes[i] != -1)
                {
                    // They voted! Write "1 | 3"
                    stickyNoteTexts[i].text = $"{i + 1} | {playerVotes[i] + 1}";
                }
                else
                {
                    // They haven't voted yet. Write "1 | "
                    stickyNoteTexts[i].text = $"{i + 1} | ";
                }
            }
        }
    }

    private void HighlightItems()
    {
        int playerCount = Mathf.Min(GameManager.Instance.playerCount, Gamepad.all.Count);

        for (int p = 0; p < playerCount; p++)
        {
            for (int b = 0; b < itemSlots.Length; b++)
            {
                if (playerHighlights[p, b] != null)
                    playerHighlights[p, b].enabled = false;
            }
        }

        for (int p = 0; p < playerCount; p++)
        {
            int current = Mathf.Clamp(currentIndexes[p], 0, itemSlots.Length - 1);
            if (playerHighlights[p, current] != null)
                playerHighlights[p, current].enabled = true;
        }
    }

    private IEnumerator StartShopTimer()
    {
        timer = 0;
        while (timer < shopTimerDurationInSecs)
        {
            timer += Time.deltaTime;

            int votesCounted = 0;
            for (int i = 0; i < playerCount; i++)
            {
                if (playerVotes[i] != -1) votesCounted++;
            }

            if (votesCounted == playerCount || amtOfItems == 0)
            {
                timer = Mathf.Max(timer, shopTimerDurationInSecs - 3f);
            }
            yield return null;
        }
        timer = shopTimerDurationInSecs;

        int currentHighestVal = -1;
        List<int> winners = new List<int>();

        for (int i = 0; i < itemVotes.Length; i++)
        {
            if (itemVotes[i] > currentHighestVal)
            {
                winners.Clear(); winners.Add(i);
                currentHighestVal = itemVotes[i];
            }
            else if (itemVotes[i] == currentHighestVal)
            {
                winners.Add(i); currentHighestVal = itemVotes[i];
            }
        }

        StartCoroutine(VisualRouletteSpin(winners));
    }

    private IEnumerator VisualRouletteSpin(List<int> winners)
    {
        int finalWinnerIndex = -1;

        if (winners.Count > 0)
        {
            if (winners.Count > 1)
            {
                float spinTime = 2.0f;
                float currentSpinTime = 0f;
                float delay = 0.1f;

                while (currentSpinTime < spinTime)
                {
                    int randomFlash = winners[Random.Range(0, winners.Count)];

                    if (playerHighlights[0, randomFlash] != null)
                    {
                        playerHighlights[0, randomFlash].enabled = true;
                        playerHighlights[0, randomFlash].color = Color.white;
                    }

                    yield return new WaitForSeconds(delay);

                    if (playerHighlights[0, randomFlash] != null)
                    {
                        playerHighlights[0, randomFlash].enabled = false;
                        playerHighlights[0, randomFlash].color = playerColors[0];
                    }

                    currentSpinTime += delay;
                    delay += 0.02f;
                }
            }

            int randIndex = Random.Range(0, winners.Count);
            finalWinnerIndex = winners[randIndex];
            addedPowerUp = powerUps[finalWinnerIndex];

            if (playerHighlights[0, finalWinnerIndex] != null)
            {
                playerHighlights[0, finalWinnerIndex].enabled = true;
                playerHighlights[0, finalWinnerIndex].color = Color.yellow;
            }

            yield return new WaitForSeconds(1.5f);
        }

        if (addedPowerUp)
            RoundManager.Instance.powerUpsInRotation.Add(addedPowerUp.powerup);

        RoundManager.Instance.switchRoundScene();
    }

    private void setupItems()
    {
        powerUps = new List<ItemData>();
        amtOfItems = itemSlots.Length;

        List<ItemData> availableItems = new List<ItemData>();
        foreach (ItemData item in powerUpRegistry)
        {
            if (!RoundManager.Instance.powerUpsInRotation.Contains(item.powerup))
            {
                availableItems.Add(item);
            }
        }

        int numItemsToSetup = Mathf.Min(itemSlots.Length, availableItems.Count);
        HashSet<int> chosenIndexes = new HashSet<int>();

        for (int i = 0; i < numItemsToSetup; i++)
        {
            int randIndex;
            ItemData randItem;

            do
            {
                randIndex = Random.Range(0, availableItems.Count);
                randItem = availableItems[randIndex];
            }
            while (chosenIndexes.Contains(randIndex));

            chosenIndexes.Add(randIndex);

            if (itemSlots.Length > i && itemSlots[i] != null && randItem.itemIcon != null)
            {
                itemSlots[i].sprite = randItem.itemIcon;
            }

            if (numberIconObjects.Length > i && numberIconObjects[i] != null)
            {
                numberIconObjects[i].SetActive(true);
            }

            powerUps.Add(randItem);
        }

        for (int i = numItemsToSetup; i < itemSlots.Length; i++)
        {
            if (itemSlots.Length > i && itemSlots[i] != null && soldOutSprite != null)
            {
                itemSlots[i].sprite = soldOutSprite;
            }

            if (numberIconObjects.Length > i && numberIconObjects[i] != null)
            {
                numberIconObjects[i].SetActive(false);
            }

            amtOfItems--;
        }
    }

    private void SetupHighlights()
    {
        int playerCount = Mathf.Min(GameManager.Instance.playerCount, Gamepad.all.Count);

        for (int p = 0; p < playerCount; p++)
        {
            for (int b = 0; b < itemSlots.Length; b++)
            {
                GameObject highlightObj = new GameObject($"Player{p + 1}_Highlight_Slot{b + 1}");

                if (numberIconObjects.Length > b && numberIconObjects[b] != null)
                {
                    highlightObj.transform.SetParent(numberIconObjects[b].transform, false);
                }
                else
                {
                    highlightObj.transform.SetParent(itemSlots[b].transform, false);
                }

                Image img = highlightObj.AddComponent<Image>();
                img.color = playerColors[p];
                img.raycastTarget = false;

                RectTransform rt = highlightObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 1);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                Color c = img.color;
                c.a = 0.40f;
                img.color = c;

                img.enabled = false;
                playerHighlights[p, b] = img;
            }
        }
    }
}