using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

[System.Serializable]
public class ShopColorData
{
    public string colorName;
    public int colorIndex;
    public Color keypadHighlight;
    public Sprite headIcon;
    public Sprite tallyMarkSprite;
}

public class ShopManager : MonoBehaviour
{
    [SerializeField] private VoteImageManager vim;

    public List<ItemData> powerUpRegistry;

    [Header("Timing")]
    [SerializeField] private float shopTimerDurationInSecs;
    public float shopIntroDuration = 2.0f; // How long the intro animation takes

    [Header("UI Elements (Images Only!)")]
    [SerializeField] private Image[] itemSlots;
    [SerializeField] private GameObject[] numberIconObjects;
    [SerializeField] private Sprite soldOutSprite;

    [SerializeField] private TMP_Text timerText;

    [Header("Audio & FX")]
    public AudioSource audioSource;
    public AudioSource bgmAudioSource;
    public AudioClip rouletteTickSound;
    public AudioClip rouletteWinnerSound;
    public AudioClip coinInsertSound;
    public Animator coinSlotAnimator;

    [Header("Vote Cast Images (The 16 Tied Slots)")]
    public Image[] item1VoteSlots;
    public Image[] item2VoteSlots;
    public Image[] item3VoteSlots;
    public Image[] item4VoteSlots;

    [Header("Player Color Database")]
    public ShopColorData[] availableColors;

    private float timer;
    private int amtOfItems;
    private int[] playerVotes;
    private bool[] isSoldOut = new bool[4];
    private bool introFinished = false;

    public Image[,] playerNumHighlights = new Image[4, 4];
    private int[] currentIndexes = new int[4];
    private bool[] canMove = new bool[4];
    private bool[] lockedIn = new bool[4];

    private int[] itemVotes;
    private List<ItemData> powerUps;
    private ItemData addedPowerUp;

    private void Start()
    {
        // --- THE FIX: Snap time back to 100% normal speed! ---
        Time.timeScale = 1f;

        // --- NEW: Automatically hide the timer when the scene starts ---
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }

        setupItems();

        int playerCount = 4;
        if (GameManager.Instance != null) playerCount = Mathf.Min(GameManager.Instance.playerCount, 4);

        itemVotes = new int[itemSlots.Length];

        for (int i = 0; i < 4; i++)
        {
            currentIndexes[i] = 0;
            canMove[i] = false;
            lockedIn[i] = false;
        }

        playerVotes = new int[4];
        for (int i = 0; i < 4; i++) playerVotes[i] = -1;

        SetupHighlights();
        UpdateVoteImages();

        // Hide highlights during the intro!
        for (int p = 0; p < 4; p++)
        {
            for (int b = 0; b < numberIconObjects.Length; b++)
            {
                if (playerNumHighlights[p, b] != null) playerNumHighlights[p, b].enabled = false;
            }
        }

        // Start the Intro sequence!
        StartCoroutine(ShopIntroSequence());
    }

    private IEnumerator ShopIntroSequence()
    {
        // 1. Wait for the vending machine animation to finish
        yield return new WaitForSeconds(shopIntroDuration);

        // 2. Unlock players and show their highlights!
        int safePlayerCount = 4;
        if (GameManager.Instance != null) safePlayerCount = Mathf.Min(GameManager.Instance.playerCount, 4);

        for (int i = 0; i < safePlayerCount; i++)
        {
            canMove[i] = true;
        }

        introFinished = true;
        HighlightKeypad();

        // --- NEW: Turn the timer visually ON right before it starts! ---
        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
        }

        // 3. Start the actual voting timer
        StartCoroutine(StartShopTimer());
    }

    private void Update()
    {
        if (introFinished)
        {
            timerText.text = $"{shopTimerDurationInSecs - (int)timer}";
            timerText.color = timer >= shopTimerDurationInSecs - 3f ? Color.red : Color.white;
        }
        else
        {
            if (timerText != null) timerText.text = "---";
        }

        HandleInput();
    }

    private ShopColorData GetPlayerColorData(int playerID)
    {
        if (GameManager.Instance != null && GameManager.Instance.playerCustomizations.Count > playerID)
        {
            int savedColorIndex = GameManager.Instance.playerCustomizations[playerID].colorIndex;

            foreach (ShopColorData data in availableColors)
            {
                if (data.colorIndex == savedColorIndex)
                {
                    return data;
                }
            }
        }
        if (availableColors.Length > 0) return availableColors[0];
        return null;
    }

    private void HandleInput()
    {
        if (!introFinished) return;
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.isOnKeyboard)
        {
            if (Keyboard.current != null && !lockedIn[0])
            {
                if (canMove[0])
                {
                    if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
                    {
                        if (currentIndexes[0] - 2 >= 0) currentIndexes[0] -= 2;
                        canMove[0] = false; HighlightKeypad();
                    }
                    else if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
                    {
                        if (currentIndexes[0] + 2 < itemSlots.Length) currentIndexes[0] += 2;
                        canMove[0] = false; HighlightKeypad();
                    }
                    else if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
                    {
                        if (currentIndexes[0] % 2 != 0) currentIndexes[0] -= 1;
                        canMove[0] = false; HighlightKeypad();
                    }
                    else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
                    {
                        if (currentIndexes[0] % 2 == 0 && currentIndexes[0] + 1 < itemSlots.Length) currentIndexes[0] += 1;
                        canMove[0] = false; HighlightKeypad();
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
                    HighlightKeypad();
                    SubmitVote(0, kVote);
                }
            }
        }
        else
        {
            int safePlayerCount = Mathf.Min(GameManager.Instance.playerCount, 4);

            for (int i = 0; i < safePlayerCount; i++)
            {
                if (lockedIn[i]) continue;

                /*int deviceId = GameManager.Instance.controllerAssignments[i];
                if (deviceId == -1) continue;

                Gamepad pad = null;

                foreach (var gp in Gamepad.all)
                {
                    if (gp.deviceId == deviceId)
                    {
                        pad = gp;
                        break;
                    }
                }

                if (pad == null) continue;

                Vector2 move = pad.leftStick.ReadValue();*/

                Gamepad pad = InputManager.Instance.GetAssignedGamepad(i + 1);

                if (pad == null)
                    continue;

                Vector2 move = pad.leftStick.ReadValue();

                if (canMove[i])
                {
                    if (move.y > 0.5f)
                    {
                        if (currentIndexes[i] - 2 >= 0) currentIndexes[i] -= 2;
                        canMove[i] = false; HighlightKeypad();
                    }
                    else if (move.y < -0.5f)
                    {
                        if (currentIndexes[i] + 2 < itemSlots.Length) currentIndexes[i] += 2;
                        canMove[i] = false; HighlightKeypad();
                    }
                    else if (move.x < -0.5f)
                    {
                        if (currentIndexes[i] % 2 != 0) currentIndexes[i] -= 1;
                        canMove[i] = false; HighlightKeypad();
                    }
                    else if (move.x > 0.5f)
                    {
                        if (currentIndexes[i] % 2 == 0 && currentIndexes[i] + 1 < itemSlots.Length)
                            currentIndexes[i] += 1;

                        canMove[i] = false;
                        HighlightKeypad();
                    }
                }

                if (Mathf.Abs(move.y) < 0.2f && Mathf.Abs(move.x) < 0.2f)
                    canMove[i] = true;

                if (pad.buttonSouth.wasPressedThisFrame)
                {
                    SubmitVote(i, currentIndexes[i]);
                }
            }

            /*for (int i = 0; i < safePlayerCount; i++)
            {
                if (lockedIn[i]) continue;

                if (GameManager.Instance.controllerAssignments == null) continue;
                if (i >= GameManager.Instance.controllerAssignments.Length) continue;

                int controllerIndex = GameManager.Instance.controllerAssignments[i];
                if (controllerIndex < 0 || controllerIndex >= Gamepad.all.Count) continue;

                var gamepad = Gamepad.all[controllerIndex];
                if (gamepad == null) continue;

                Vector2 move = gamepad.leftStick.ReadValue();

                if (canMove[i])
                {
                    if (move.y > 0.5f)
                    {
                        if (currentIndexes[i] - 2 >= 0) currentIndexes[i] -= 2;
                        canMove[i] = false; HighlightKeypad();
                    }
                    else if (move.y < -0.5f)
                    {
                        if (currentIndexes[i] + 2 < itemSlots.Length) currentIndexes[i] += 2;
                        canMove[i] = false; HighlightKeypad();
                    }
                    else if (move.x < -0.5f)
                    {
                        if (currentIndexes[i] % 2 != 0) currentIndexes[i] -= 1;
                        canMove[i] = false; HighlightKeypad();
                    }
                    else if (move.x > 0.5f)
                    {
                        if (currentIndexes[i] % 2 == 0 && currentIndexes[i] + 1 < itemSlots.Length) currentIndexes[i] += 1;
                        canMove[i] = false; HighlightKeypad();
                    }
                }

                if (Mathf.Abs(move.y) < 0.2f && Mathf.Abs(move.x) < 0.2f) canMove[i] = true;

                if (gamepad.buttonSouth.wasPressedThisFrame)
                {
                    SubmitVote(i, currentIndexes[i]);
                }
            }*/
        }
    }

    private void SubmitVote(int pIndex, int vIndex)
    {
        if (vIndex < powerUps.Count && !isSoldOut[vIndex])
        {
            if (audioSource != null && coinInsertSound != null)
            {
                audioSource.PlayOneShot(coinInsertSound);
            }
            if (coinSlotAnimator != null)
            {
                coinSlotAnimator.SetTrigger("CoinInserted");
            }

            int oldVote = playerVotes[pIndex];
            if (oldVote != -1) itemVotes[oldVote]--;

            itemVotes[vIndex]++;
            playerVotes[pIndex] = vIndex;

            lockedIn[pIndex] = true;

            for (int b = 0; b < numberIconObjects.Length; b++)
            {
                if (playerNumHighlights[pIndex, b] != null) playerNumHighlights[pIndex, b].enabled = false;
            }

            if (vim != null) vim.changeVote(pIndex, vIndex);

            UpdateVoteImages();
        }
    }

    private void UpdateVoteImages()
    {
        HideAllSlots(item1VoteSlots);
        HideAllSlots(item2VoteSlots);
        HideAllSlots(item3VoteSlots);
        HideAllSlots(item4VoteSlots);

        FillVoteSlots(0, item1VoteSlots);
        FillVoteSlots(1, item2VoteSlots);
        FillVoteSlots(2, item3VoteSlots);
        FillVoteSlots(3, item4VoteSlots);
    }

    private void HideAllSlots(Image[] slots)
    {
        foreach (Image img in slots)
        {
            if (img != null) img.enabled = false;
        }
    }

    private void FillVoteSlots(int itemRowIndex, Image[] rowSlots)
    {
        int tieCounter = 0;
        int safePlayerCount = 4;
        if (GameManager.Instance != null) safePlayerCount = Mathf.Min(GameManager.Instance.playerCount, 4);

        for (int pIndex = 0; pIndex < safePlayerCount; pIndex++)
        {
            if (playerVotes[pIndex] == itemRowIndex)
            {
                if (tieCounter < rowSlots.Length && rowSlots[tieCounter] != null)
                {
                    rowSlots[tieCounter].enabled = true;

                    ShopColorData pData = GetPlayerColorData(pIndex);
                    if (pData != null && pData.headIcon != null)
                    {
                        rowSlots[tieCounter].sprite = pData.headIcon;
                    }
                }
                tieCounter++;
            }
        }
    }

    private void SetupHighlights()
    {
        int safePlayerCount = 4;
        if (GameManager.Instance != null) safePlayerCount = Mathf.Min(GameManager.Instance.playerCount, 4);

        for (int p = 0; p < safePlayerCount; p++)
        {
            ShopColorData pData = GetPlayerColorData(p);
            Color playerCustomColor = (pData != null) ? pData.keypadHighlight : Color.white;

            for (int b = 0; b < numberIconObjects.Length; b++)
            {
                if (numberIconObjects.Length > b && numberIconObjects[b] != null)
                {
                    GameObject numHighlightObj = new GameObject($"Player{p + 1}_Highlight_Num{b + 1}");
                    numHighlightObj.transform.SetParent(numberIconObjects[b].transform, false);
                    Image numImg = numHighlightObj.AddComponent<Image>();

                    numImg.color = playerCustomColor;
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

    private void HighlightKeypad()
    {
        if (!introFinished) return;

        int safePlayerCount = 4;
        if (GameManager.Instance != null) safePlayerCount = Mathf.Min(GameManager.Instance.playerCount, 4);

        for (int p = 0; p < safePlayerCount; p++)
        {
            if (!lockedIn[p])
            {
                for (int b = 0; b < numberIconObjects.Length; b++)
                {
                    if (playerNumHighlights[p, b] != null) playerNumHighlights[p, b].enabled = false;
                }
            }
        }

        for (int p = 0; p < safePlayerCount; p++)
        {
            if (!lockedIn[p])
            {
                int current = Mathf.Clamp(currentIndexes[p], 0, itemSlots.Length - 1);
                if (playerNumHighlights[p, current] != null) playerNumHighlights[p, current].enabled = true;
            }
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

            int safePlayerCount = 4;
            if (GameManager.Instance != null) safePlayerCount = Mathf.Min(GameManager.Instance.playerCount, 4);

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

                    if (audioSource != null && rouletteTickSound != null)
                        audioSource.PlayOneShot(rouletteTickSound);

                    if (itemSlots[randomFlash] != null)
                        itemSlots[randomFlash].color = Color.gray;

                    yield return new WaitForSeconds(delay);

                    if (itemSlots[randomFlash] != null)
                        itemSlots[randomFlash].color = Color.white;

                    currentSpinTime += delay;
                    delay += 0.02f;
                }
            }

            int randIndex = Random.Range(0, winners.Count);
            finalWinnerIndex = winners[randIndex];
            addedPowerUp = powerUps[finalWinnerIndex];

            if (audioSource != null && rouletteWinnerSound != null)
                audioSource.PlayOneShot(rouletteWinnerSound);

            if (itemSlots[finalWinnerIndex] != null)
                itemSlots[finalWinnerIndex].color = Color.green;

            float fadeTime = 1.5f;
            float elapsed = 0f;
            float startVol = (bgmAudioSource != null) ? bgmAudioSource.volume : 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                if (bgmAudioSource != null)
                {
                    bgmAudioSource.volume = Mathf.Lerp(startVol, 0f, elapsed / fadeTime);
                }
                yield return null;
            }
        }

        if (addedPowerUp != null)
        {
            if (RoundManager.Instance != null && RoundManager.Instance.powerUpsInRotation != null)
            {
                RoundManager.Instance.powerUpsInRotation.Add(addedPowerUp.powerup);
            }
        }

        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.switchRoundScene();
        }
    }

    private void setupItems()
    {
        powerUps = new List<ItemData>();
        amtOfItems = itemSlots.Length;

        List<ItemData> availableItems = new List<ItemData>();
        foreach (ItemData item in powerUpRegistry)
        {
            if (RoundManager.Instance == null || !RoundManager.Instance.powerUpsInRotation.Contains(item.powerup))
            {
                availableItems.Add(item);
            }
        }

        List<ItemData> shuffledItems = new List<ItemData>(availableItems);
        for (int i = 0; i < shuffledItems.Count; i++)
        {
            ItemData temp = shuffledItems[i];
            int randomIndex = Random.Range(i, shuffledItems.Count);
            shuffledItems[i] = shuffledItems[randomIndex];
            shuffledItems[randomIndex] = temp;
        }

        int numItemsToSetup = Mathf.Min(itemSlots.Length, shuffledItems.Count);

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (i < numItemsToSetup)
            {
                ItemData chosenItem = shuffledItems[i];
                powerUps.Add(chosenItem);
                isSoldOut[i] = false;

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
}