using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuDirector : MonoBehaviour
{
    [Header("Logo")]
    [SerializeField] private RectTransform logoRT;     // the logo RectTransform
    [SerializeField] private CanvasGroup logoCG;       // add CanvasGroup to the logo
    [SerializeField] private float logoDropPixels = 60f;
    [SerializeField] private float logoFadeTime = 0.6f;
    [SerializeField] private float logoHoldTime = 0.6f;

    [Header("Play Button")]
    [SerializeField] private Button playButton;        // your Play button

    [Header("Word Images (start hidden)")]
    [SerializeField] private GameObject localGO;       // Image GameObject for "LOCAL"
    [SerializeField] private GameObject multiplayerGO; // Image GameObject for "MULTIPLAYER"
    [SerializeField] private float popStagger = 0.12f; // tiny delay between them

    private void Awake()
    {
        // start with words hidden and non-interactable
        PrepareWord(localGO);
        PrepareWord(multiplayerGO);
    }

    private void Start()
    {
        // run the logo intro
        StartCoroutine(LogoIntro());
        playButton.onClick.AddListener(OnPlayPressed);
    }

    private IEnumerator LogoIntro()
    {
        // slide down + fade in
        Vector2 targetPos = logoRT.anchoredPosition;
        Vector2 startPos  = targetPos + new Vector2(0f, logoDropPixels);
        logoRT.anchoredPosition = startPos;
        logoCG.alpha = 0f;

        float t = 0f;
        while (t < logoFadeTime)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / logoFadeTime);
            logoCG.alpha = a;
            // ease-out slide
            float eased = 1f - Mathf.Pow(1f - a, 3f);
            logoRT.anchoredPosition = Vector2.Lerp(startPos, targetPos, eased);
            yield return null;
        }
        logoCG.alpha = 1f;
        logoRT.anchoredPosition = targetPos;

        // small hold/pause
        if (logoHoldTime > 0f) yield return new WaitForSecondsRealtime(logoHoldTime);
    }

    private void OnPlayPressed()
    {
        // reveal the two word images with a pop-in
        StartCoroutine(PopIn(localGO, 0f));
        StartCoroutine(PopIn(multiplayerGO, popStagger));
    }

    // --- helpers ---

    private void PrepareWord(GameObject go)
    {
        if (!go) return;
        var cg = go.GetComponent<CanvasGroup>();
        if (!cg) cg = go.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        var rt = go.GetComponent<RectTransform>();
        if (rt) rt.localScale = Vector3.one * 0.85f;

        // disable click handlers until shown
        ToggleWordInteractable(go, false);
        go.SetActive(false);
    }

    private IEnumerator PopIn(GameObject go, float delay)
    {
        if (!go) yield break;
        yield return new WaitForSecondsRealtime(delay);

        go.SetActive(true);

        var cg = go.GetComponent<CanvasGroup>();
        var rt = go.GetComponent<RectTransform>();

        float dur = 0.22f;
        float t = 0f;
        float startA = 0f;
        float endA = 1f;
        Vector3 startS = Vector3.one * 0.85f;
        Vector3 endS   = Vector3.one;

        if (rt) rt.localScale = startS;
        if (cg) cg.alpha = startA;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / dur);
            // ease-out cubic for a snappy pop
            float eased = 1f - Mathf.Pow(1f - a, 3f);

            if (cg) cg.alpha = Mathf.Lerp(startA, endA, eased);
            if (rt) rt.localScale = Vector3.Lerp(startS, endS, eased);
            yield return null;
        }

        if (cg) cg.alpha = endA;
        if (rt) rt.localScale = endS;
        ToggleWordInteractable(go, true);
    }

    private void ToggleWordInteractable(GameObject go, bool on)
    {
        // If you're using the ImageButtonSwitch from earlier, enable/disable raycasts here
        var img = go.GetComponent<UnityEngine.UI.Image>();
        if (img) img.raycastTarget = on;

        var selectable = go.GetComponent<Selectable>();
        if (selectable) selectable.interactable = on;
    }
}
