using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FakeLoadingScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject loadingPanel;
    public Image loadingBarFill;
    public GameObject pressAButtonPrompt;

    [Header("Timing")]
    public float loadDuration = 10f;

    private float timer = 0f;
    private bool loadingComplete = false;

    private void Start()
    {
        // Disable jump for all players during loading
        foreach (var player in FindObjectsByType<Playermovement>(FindObjectsSortMode.None))
        {
            player.allowJumpInput = false;
        }


        loadingPanel.SetActive(true);
        pressAButtonPrompt.SetActive(false);
        loadingBarFill.fillAmount = 0;

        timer = 0f;
        loadingComplete = false;
    }

    private void Update()
    {
        // STEP 1: Animate loading bar
        if (!loadingComplete)
        {
            timer += Time.deltaTime;
            loadingBarFill.fillAmount = timer / loadDuration;

            if (timer >= loadDuration)
            {
                loadingComplete = true;
                pressAButtonPrompt.SetActive(true);
            }
            return;
        }

        // STEP 2: Wait for "A" on ANY player's gamepad
        foreach (var pad in UnityEngine.InputSystem.Gamepad.all)
        {
            if (pad.buttonSouth.wasPressedThisFrame)
            {
                BeginRound();
                break;
            }
        }
    }

    private void BeginRound()
    {
        RoundManager.Instance.BeginRound();
        loadingPanel.SetActive(false);
        this.enabled = false; // prevent duplicate calls
    }

}
