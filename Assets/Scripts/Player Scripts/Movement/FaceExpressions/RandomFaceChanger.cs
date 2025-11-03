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
    public float maxChangeTime = 3f;

    private bool isChanging = true;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

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
            if (faceAnimations.Length > 0)
            {
                int randomIndex = Random.Range(0, faceAnimations.Length);
                string chosenFace = faceAnimations[randomIndex];
                animator.Play(chosenFace);
            }
        }
    }

    public void StopChangingFaces()
    {
        isChanging = false;
        StopAllCoroutines();
    }
}
