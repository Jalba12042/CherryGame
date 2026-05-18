using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class BackInputToAnimatorTrigger : MonoBehaviour
{
    [Header("Animators that should play the EXIT animation")]
    public Animator controllerLayoutAnimator;   // big layout image (has leave state, e.g., "ControllerLeave")
    public Animator backImageAnimator;          // the BACK image (has leave state, e.g., "BackButtonFadeOut")
    public string exitTrigger = "Exit";         // Trigger name in both animators

    [Header("Scene to load AFTER exit")]
    public string sceneToLoad = "Main Menu";

    [Header("When can BACK be pressed?")]
    public bool armOnStart = false;             // set true to allow input after a short delay
    public float delayBeforeInput = 0.8f;       // used if armOnStart = true

    [Header("How long to wait for the exit anims (if not using events)")]
    public float exitWaitSeconds = 1.0f;        // set to the longest exit clip length

    // (Optional) If you want more robust waiting by state names, fill these with your leave state names
    [Header("Optional exact state names (leave empty to use time wait)")]
    public string controllerLeaveState = "";    // e.g., "ControllerLeave"
    public string backLeaveState = "";          // e.g., "BackButtonFadeOut"
    public float stateWaitTimeout = 2.0f;

    private bool inputArmed = false;
    private bool exiting = false;

    void Start()
    {
        if (armOnStart)
            StartCoroutine(ArmAfterDelay(delayBeforeInput));
        // else: call ArmBack() via an Animation Event at the end of your intro
    }

    IEnumerator ArmAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ArmBack();
    }

    // Call this from the end of your ControllerLayout "intro" clip via Animation Event
    public void ArmBack()
    {
        inputArmed = true;
        // Debug.Log("[BackInput] Back armed.");
    }

    void Update()
    {
        if (!inputArmed || exiting) return;

        if (BackPressed())
        {
            exiting = true;
            // fire Exit triggers
            if (controllerLayoutAnimator) controllerLayoutAnimator.SetTrigger(exitTrigger);
            if (backImageAnimator) backImageAnimator.SetTrigger(exitTrigger);

            // wait for exit to finish (by state if given, else by time)
            if (!string.IsNullOrEmpty(controllerLeaveState) || !string.IsNullOrEmpty(backLeaveState))
                StartCoroutine(WaitByStatesThenLoad());
            else
                StartCoroutine(WaitByTimeThenLoad());
        }
    }

    bool BackPressed()
    {
        return InputManager.Instance != null && InputManager.Instance.GetBackDown(1);
    }

    IEnumerator WaitByTimeThenLoad()
    {
        yield return new WaitForSeconds(exitWaitSeconds);
        SceneManager.LoadScene(sceneToLoad);
    }

    IEnumerator WaitByStatesThenLoad()
    {
        float t = 0f;
        bool controllerDone = string.IsNullOrEmpty(controllerLeaveState);
        bool backDone = string.IsNullOrEmpty(backLeaveState);

        while (t < stateWaitTimeout && !(controllerDone && backDone))
        {
            if (!controllerDone && PlayedAndFinished(controllerLayoutAnimator, controllerLeaveState))
                controllerDone = true;

            if (!backDone && PlayedAndFinished(backImageAnimator, backLeaveState))
                backDone = true;

            t += Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadScene(sceneToLoad);
    }

    bool PlayedAndFinished(Animator a, string stateName)
    {
        if (!a || string.IsNullOrEmpty(stateName)) return true;
        var info = a.GetCurrentAnimatorStateInfo(0);
        return info.IsName(stateName) && info.normalizedTime >= 1f;
    }
}
