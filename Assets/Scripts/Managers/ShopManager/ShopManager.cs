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

    [SerializeField] private TMP_Text[] stickyNoteTexts;
    [SerializeField] private TMP_Text timerText;

    private float timer;
    private int amtOfItems;
    private int[] playerVotes;
    private bool[] isSoldOut = new bool[4];

    [Header("Highlight Images")]
    public Image[,] playerItemHighlights = new Image[4, 4];
    public Image[,] playerNumHighlights = new Image[4, 4];

    private int[] currentIndexes = new int[4];
    private bool[] canMove = new bool[4];
    private int[] itemVotes;
    private List<ItemData> powerUps;
    private ItemData addedPowerUp;

    private Color[] playerColors = { Color.blue, Color.red, Color.green, Color.yellow };

    private void Start()
    {
        setupItems();

        int playerCount = Mathf.Min(GameManager.Instance.playerCount, 4);
        itemVotes = new int[itemSlots.Length];

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

        HandleInput();
    }

    private void HandleInput()
    {
        if (GameManager.Instance.isOnKeyboard)
        {
            if (Keyboard.current != null)
            {
                if (canMove[0])
                {
                    if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
                    {
                        if (currentIndexes[0] - 2 >= 0) currentIndexes[0] -= 2;
                        canMove[0] = false; HighlightItems();
                    }
                    else if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
                    {
                        if (currentIndexes[0] + 2 < itemSlots.Length) currentIndexes[0] += 2;
                        canMove[0] = false; HighlightItems();
                    }
                    else if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
                    {
                        if (currentIndexes[0] % 2 != 0) currentIndexes[0] -= 1;
                        canMove[0] = false; HighlightItems();
                    }
                    else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
                    {
                        if (currentIndexes[0] % 2 == 0 && currentIndexes[0] + 1 < itemSlots.Length) currentIndexes[0] += 1;
                        canMove[0] = false; HighlightItems();
                    }
                }

                if (!Keyboard.current.wKey.isPressed && !Keyboard.current.sKey.isPressed &&
                    !Keyboard.current.aKey.isPressed && !Keyboard.current.dKey.isPressed &&
                    !Keyboard.current.upArrowKey.isPressed && !Keyboard.current.downArrowKey.isPressed &&
                    !Keyboard.current.leftArrowKey.isPressed && !Keyboard.current.rightArrowKey.isPressed)
                {
                    canMove[0] = true;
                }

                if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    SubmitVote(0, currentIndexes[0]);
                }

                int kVote = -1;
                if (Keyboard.current.digit1Key.wasPressedThisFrame) kVote = 0;
                if (Keyboard.current.digit2Key.wasPressedThisFrame) kVote = 1;
                if (Keyboard.current.digit3Key.wasPressedThisFrame) kVote = 2;
                if (Keyboard.current.digit4Key.wasPressedThisFrame) kVote = 3;

                if (kVote != -1 && kVote < isSoldOut.Length && !isSoldOut[kVote])
                {
                    currentIndexes[0] = kVote;
                    HighlightItems();
                    SubmitVote(0, kVote);
                }
            }
        }
        else
        {
            int safePlayerCount = Mathf.Min(GameManager.Instance.playerCount, 4);

            for (int i = 0; i < safePlayerCount; i++)
            {
                if (i >= GameManager.Instance.controllerAssignments.Length) continue;

                int controllerIndex = GameManager.Instance.controllerAssignments[i];
                if (controllerIndex < 0 || controllerIndex >= Gamepad.all.Count) continue;

                var gamepad = Gamepad.all[controllerIndex];
                Vector2 move = gamepad.leftStick.ReadValue();

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

                if (Mathf.Abs(move.y) < 0.2f && Mathf.Abs(move.x) < 0.2f) canMove[i] = true;

                if (gamepad.buttonSouth.wasPressedThisFrame)
                {
                    SubmitVote(i, currentIndexes[i]);
                }
            }
        }
    }

    private void SubmitVote(int pIndex, int vIndex)
    {
        if (vIndex < powerUps.Count && !isSoldOut[vIndex])
        {
            int oldVote = playerVotes[pIndex];
            if (oldVote != -1) itemVotes[oldVote]--;

            itemVotes[vIndex]++;
            playerVotes[pIndex] = vIndex;

            if (vim != null) vim.changeVote(pIndex, vIndex);
            UpdateStickyNote();
        }
    }

    private void UpdateStickyNote()
    {
        for (int i = 0; i < 4; i++)
        {
            if (i < stickyNoteTexts.Length && stickyNoteTexts[i] != null)
            {
                if (playerVotes[i] != -1)
                    stickyNoteTexts[i].text = $"{i + 1} | L";
                else
                    stickyNoteTexts[i].text = $"{i + 1} | ";
            }
        }
    }

    private void HighlightItems()
    {
        int safePlayerCount = Mathf.Min(GameManager.Instance.playerCount, 4);

        for (int p = 0; p < safePlayerCount; p++)
        {
            for (int b = 0; b < itemSlots.Length; b++)
            {
                if (playerItemHighlights[p, b] != null) playerItemHighlights[p, b].enabled = false;
                if (playerNumHighlights[p, b] != null) playerNumHighlights[p, b].enabled = false;
            }
        }

        for (int p = 0; p < safePlayerCount; p++)
        {
            int current = Mathf.Clamp(currentIndexes[p], 0, itemSlots.Length - 1);
            if (playerItemHighlights[p, current] != null) playerItemHighlights[p, current].enabled = true;
            if (playerNumHighlights[p, current] != null) playerNumHighlights[p, current].enabled = true;
        }
    }

    private IEnumerator StartShopTimer()
    {
        timer = 0;
        while (timer < shopTimerDurationInSecs)
        {
            timer += Time.deltaTime;
            int votesCounted = 0;
            int availableItems = 0;

            int safePlayerCount = Mathf.Min(GameManager.Instance.playerCount, 4);
            for (int i = 0; i < safePlayerCount; i++)
            {
                if (playerVotes[i] != -1) votesCounted++;
            }

            for (int i = 0; i < isSoldOut.Length; i++)
            {
                if (!isSoldOut[i]) availableItems++;
            }

            if (votesCounted == safePlayerCount || availableItems == 0)
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

                    if (playerItemHighlights[0, randomFlash] != null)
                    {
                        playerItemHighlights[0, randomFlash].enabled = true;
                        playerItemHighlights[0, randomFlash].color = Color.white;
                    }
                    if (playerNumHighlights[0, randomFlash] != null)
                    {
                        playerNumHighlights[0, randomFlash].enabled = true;
                        playerNumHighlights[0, randomFlash].color = Color.white;
                    }

                    yield return new WaitForSeconds(delay);

                    if (playerItemHighlights[0, randomFlash] != null)
                    {
                        playerItemHighlights[0, randomFlash].enabled = false;
                        playerItemHighlights[0, randomFlash].color = playerColors[0];
                    }
                    if (playerNumHighlights[0, randomFlash] != null)
                    {
                        playerNumHighlights[0, randomFlash].enabled = false;
                        playerNumHighlights[0, randomFlash].color = playerColors[0];
                    }

                    currentSpinTime += delay;
                    delay += 0.02f;
                }
            }

            int randIndex = Random.Range(0, winners.Count);
            finalWinnerIndex = winners[randIndex];
            addedPowerUp = powerUps[finalWinnerIndex];

            if (playerItemHighlights[0, finalWinnerIndex] != null)
            {
                playerItemHighlights[0, finalWinnerIndex].enabled = true;
                playerItemHighlights[0, finalWinnerIndex].color = Color.yellow;
            }
            if (playerNumHighlights[0, finalWinnerIndex] != null)
            {
                playerNumHighlights[0, finalWinnerIndex].enabled = true;
                playerNumHighlights[0, finalWinnerIndex].color = Color.yellow;
            }

            yield return new WaitForSeconds(1.5f);
        }

        if (addedPowerUp != null)
        {
            RoundManager.Instance.powerUpsInRotation.Add(addedPowerUp.powerup);
        }

        RoundManager.Instance.switchRoundScene();
    }

    private void setupItems()
    {
        powerUps = new List<ItemData>();
        amtOfItems = itemSlots.Length;

        // 1. Gather what items are available
        List<ItemData> availableItems = new List<ItemData>();
        foreach (ItemData item in powerUpRegistry)
        {
            if (!RoundManager.Instance.powerUpsInRotation.Contains(item.powerup))
            {
                availableItems.Add(item);
            }
        }

        // 2. Randomly shuffle the available items!
        List<ItemData> shuffledItems = new List<ItemData>(availableItems);
        for (int i = 0; i < shuffledItems.Count; i++)
        {
            ItemData temp = shuffledItems[i];
            int randomIndex = Random.Range(i, shuffledItems.Count);
            shuffledItems[i] = shuffledItems[randomIndex];
            shuffledItems[randomIndex] = temp;
        }

        int numItemsToSetup = Mathf.Min(itemSlots.Length, shuffledItems.Count);

        // 3. Lock the shuffled items into the visual slots
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (i < numItemsToSetup)
            {
                ItemData chosenItem = shuffledItems[i];
                powerUps.Add(chosenItem);
                isSoldOut[i] = false;

                // THIS is what changes the picture in the frame!
                if (itemSlots[i] != null && chosenItem.itemIcon != null)
                {
                    itemSlots[i].sprite = chosenItem.itemIcon;
                }

                if (numberIconObjects.Length > i && numberIconObjects[i] != null)
                {
                    numberIconObjects[i].SetActive(true);
                }
            }
            else
            {
                // Fill the rest with Sold Out signs
                isSoldOut[i] = true;
                powerUps.Add(null);

                if (itemSlots[i] != null && soldOutSprite != null)
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
    }

    private void SetupHighlights()
    {
        int safePlayerCount = Mathf.Min(GameManager.Instance.playerCount, 4);

        for (int p = 0; p < safePlayerCount; p++)
        {
            for (int b = 0; b < itemSlots.Length; b++)
            {
                GameObject itemHighlightObj = new GameObject($"Player{p + 1}_Highlight_Item{b + 1}");
                itemHighlightObj.transform.SetParent(itemSlots[b].transform, false);
                Image itemImg = itemHighlightObj.AddComponent<Image>();
                itemImg.color = playerColors[p];
                itemImg.raycastTarget = false;
                RectTransform rtItem = itemHighlightObj.GetComponent<RectTransform>();
                rtItem.anchorMin = Vector2.zero; rtItem.anchorMax = Vector2.one;
                rtItem.offsetMin = Vector2.zero; rtItem.offsetMax = Vector2.zero;
                Color cItem = itemImg.color; cItem.a = 0.40f; itemImg.color = cItem;
                itemImg.enabled = false;
                playerItemHighlights[p, b] = itemImg;

                if (numberIconObjects.Length > b && numberIconObjects[b] != null)
                {
                    GameObject numHighlightObj = new GameObject($"Player{p + 1}_Highlight_Num{b + 1}");
                    numHighlightObj.transform.SetParent(numberIconObjects[b].transform, false);
                    Image numImg = numHighlightObj.AddComponent<Image>();
                    numImg.color = playerColors[p];
                    numImg.raycastTarget = false;
                    RectTransform rtNum = numHighlightObj.GetComponent<RectTransform>();
                    rtNum.anchorMin = Vector2.zero; rtNum.anchorMax = Vector2.one;
                    rtNum.offsetMin = Vector2.zero; rtNum.offsetMax = Vector2.zero;
                    Color cNum = numImg.color; cNum.a = 0.40f; numImg.color = cNum;
                    numImg.enabled = false;
                    playerNumHighlights[p, b] = numImg;
                }
            }
        }
    }
}