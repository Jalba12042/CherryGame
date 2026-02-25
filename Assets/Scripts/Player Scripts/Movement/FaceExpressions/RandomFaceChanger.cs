using UnityEngine;
using System.Collections;

public class RandomFaceChanger : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerIndex = 0;

    [Header("Face Settings")]
    public Animator animator;
    public string[] faceAnimations; // Drag animation state names here
    public float minChangeTime = 1f;
    public float maxChangeTime = 5f;

    private bool isChanging = true;

    void Start()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        

        StartCoroutine(ChangeFaceLoop());
    }

    private IEnumerator ChangeFaceLoop()
    {
        while (isChanging)
        {
            // Wait a random time before switching
            float waitTime = Random.Range(minChangeTime, maxChangeTime);
            yield return new WaitForSeconds(waitTime);

            // Pick a random animation
            if (faceAnimations.Length > 0 && !animator.GetCurrentAnimatorStateInfo(0).IsName("Scream(MouthOpen"))
            {
                int randomIndex = Random.Range(0, faceAnimations.Length);
                string chosenFace = faceAnimations[randomIndex];
                animator.Play(chosenFace);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void StopChangingFaces()
    {
        isChanging = false;
        StopAllCoroutines();
    }

    public void PauseFaces()
    {
        isChanging = false;
        StopAllCoroutines();
    }


    public void ResumeFaces()
    {
        if (!isChanging)
        {
            isChanging = true;
            StartCoroutine(ChangeFaceLoop());
        }
    }

}
