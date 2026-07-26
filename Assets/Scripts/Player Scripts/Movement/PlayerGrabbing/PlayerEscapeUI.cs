using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerEscapeUI : MonoBehaviour
{
    [Header("Escape Settings")]
    public float mashFillSpeed = 0.2f;

    [Tooltip("Base amount of mash required when one player is grabbing you.")]
    public float escapeThreshold = 1f;

    [Tooltip("How much harder escaping becomes per additional grabber.")]
    public float escapeDifficultyPerGrabber = 1f;


    [Header("Player Info")]
    public int playerIndex = 0;


    [Header("UI")]
    public GameObject panelRoot;


    private Image fillBar;
    private TextMeshProUGUI mashText;
    private Image yButtonIcon;


    private float fillAmount = 0f;

    private bool isBeingGrabbed = false;

    private PlayerInteract playerInteract;


    private void Start()
    {
        playerInteract =
            GetComponent<PlayerInteract>();

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }


    // =========================================================
    // START BEING GRABBED
    // =========================================================

    public void StartBeingGrabbed()
    {
        isBeingGrabbed = true;

        fillAmount = 0f;


        if (panelRoot != null)
            panelRoot.SetActive(true);


        CacheUI();


        if (fillBar != null)
            fillBar.fillAmount = 0f;
    }


    // =========================================================
    // CACHE UI
    // =========================================================

    private void CacheUI()
    {
        if (panelRoot == null)
            return;

        fillBar =
            panelRoot.transform
                .Find("FillBar")
                ?.GetComponent<Image>();

        mashText =
            panelRoot.transform
                .Find("MashText")
                ?.GetComponent<TextMeshProUGUI>();

        yButtonIcon =
            panelRoot.transform
                .Find("YButton Image")
                ?.GetComponent<Image>();
    }


    // =========================================================
    // STOP BEING GRABBED
    // =========================================================

    public void StopBeingGrabbed()
    {
        isBeingGrabbed = false;

        fillAmount = 0f;


        if (fillBar != null)
            fillBar.fillAmount = 0f;


        if (panelRoot != null)
            panelRoot.SetActive(false);
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!isBeingGrabbed)
            return;

        if (playerInteract == null)
            return;


        int grabberCount =
            playerInteract.NumberOfGrabbers;


        if (grabberCount <= 0)
        {
            StopBeingGrabbed();
            return;
        }


        // =====================================================
        // ESCAPE DIFFICULTY
        // =====================================================

        float currentEscapeThreshold =
            escapeThreshold +
            ((grabberCount - 1) *
            escapeDifficultyPerGrabber);


        // =====================================================
        // MASH Y
        // =====================================================

        if (InputManager.Instance.GetEscapeDown(
            playerIndex + 1))
        {
            fillAmount += mashFillSpeed;


            fillAmount =
                Mathf.Clamp(
                    fillAmount,
                    0f,
                    currentEscapeThreshold);


            if (fillBar != null)
            {
                fillBar.fillAmount =
                    fillAmount /
                    currentEscapeThreshold;
            }


            if (fillAmount >=
                currentEscapeThreshold)
            {
                Escape();
            }
        }
    }


    // =========================================================
    // ESCAPE
    // =========================================================

    private void Escape()
    {
        if (playerInteract == null)
            return;


        StopBeingGrabbed();


        // Releases this player from EVERYONE
        playerInteract.EscapeFromAllGrabbers();
    }
}