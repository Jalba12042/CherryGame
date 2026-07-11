using UnityEngine;
using System.Collections;
using TMPro;

public class AwardShowManager : MonoBehaviour
{
    [Header("Stage Elements")]
    public Animator stageAnimator; // Drag your AwardShowStage here

    void Start()
    {
        // Start the sequence the moment the scene loads!
        StartCoroutine(AwardShowSequence());
    }

    private IEnumerator AwardShowSequence()
    {
        // 1. Wait a brief second for the scene to load smoothly
        yield return new WaitForSeconds(1f);

        // 2. Trigger the curtain animation you just made
        if (stageAnimator != null)
        {
            stageAnimator.Play("CurtainReveal");
        }

        // 3. Wait for the curtains to finish opening (1.5 seconds)
        yield return new WaitForSeconds(1.5f);

        // NEXT PHASE GOES HERE:
        // This is where we will eventually spawn the Host and show the dialogue!
        Debug.Log("Curtains are open! Bring out the Host!");
    }
}