using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class AwardShowManager : MonoBehaviour
{
    [Header("Stage Elements")]
    public Animator stageAnimator;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip drumrollClip;
    public AudioClip cheerClip;
    public AudioClip sadClip; // For the 4th place loser!

    [Header("Puppet Prefabs")]
    public GameObject firstPlacePrefab;
    public GameObject secondPlacePrefab;
    public GameObject thirdPlacePrefab;
    public GameObject fourthPlacePrefab;

    [Header("Puppet Anchors")]
    public Transform anchor1st;
    public Transform anchor2nd;
    public Transform anchor3rd;
    public Transform anchor4th;

    [Header("Player Data")]
    public Sprite[] colorIcons; // To put the right colored head on the puppet

    // This helps us keep track of who got what score
    private class PlayerRank
    {
        public int playerID;
        public int score;
    }

    void Start()
    {
        StartCoroutine(AwardShowSequence());
    }

    private IEnumerator AwardShowSequence()
    {
        yield return new WaitForSeconds(1f);

        if (stageAnimator != null)
        {
            // Updated to match the animation name in your screenshot!
            stageAnimator.Play("CurtainsIntro");
        }

        yield return new WaitForSeconds(1.5f);

        // --- STEP 2: THE REVEALS ---
        yield return StartCoroutine(RevealPuppetsRoutine());

        // (Step 3 will go here later when we build the Play Again/Quit menu)
    }

    private IEnumerator RevealPuppetsRoutine()
    {
        List<PlayerRank> rankings = new List<PlayerRank>();
        int totalPlayers = 4;

        // 1. Grab the scores from the GameManager
        if (GameManager.Instance != null)
        {
            totalPlayers = GameManager.Instance.playerCount;
            for (int i = 0; i < totalPlayers; i++)
            {
                rankings.Add(new PlayerRank { playerID = i, score = GameManager.Instance.playerTotalScores[i] });
            }
        }
        else
        {
            // Fallback just in case you are testing the scene without the GameManager
            for (int i = 0; i < 4; i++) rankings.Add(new PlayerRank { playerID = i, score = Random.Range(0, 4) });
        }

        // 2. Sort the players from Highest Score to Lowest Score
        rankings.Sort((p1, p2) => p2.score.CompareTo(p1.score));

        // 3. Loop backwards to reveal 4th, then 3rd, 2nd, 1st
        for (int i = rankings.Count - 1; i >= 0; i--)
        {
            int rank = i + 1; // This gives us 1, 2, 3, or 4
            PlayerRank pData = rankings[i];

            // Play Drumroll
            if (audioSource != null && drumrollClip != null)
            {
                audioSource.PlayOneShot(drumrollClip);
            }

            // Wait for the tension to build!
            yield return new WaitForSeconds(1.5f);

            // Figure out which prefab and anchor to use based on their rank
            GameObject prefabToSpawn = null;
            Transform targetAnchor = null;
            AudioClip reactionClip = cheerClip;

            if (rank == 4) { prefabToSpawn = fourthPlacePrefab; targetAnchor = anchor4th; reactionClip = sadClip; }
            else if (rank == 3) { prefabToSpawn = thirdPlacePrefab; targetAnchor = anchor3rd; }
            else if (rank == 2) { prefabToSpawn = secondPlacePrefab; targetAnchor = anchor2nd; }
            else if (rank == 1) { prefabToSpawn = firstPlacePrefab; targetAnchor = anchor1st; }

            // Spawn the cardboard puppet!
            if (prefabToSpawn != null && targetAnchor != null)
            {
                // Spawn it exactly at the anchor position
                GameObject spawnedPuppet = Instantiate(prefabToSpawn, targetAnchor.position, targetAnchor.rotation, targetAnchor);

                // Find the blank "PlayerHead" we set up and paste the correct colored face on it
                Transform headTransform = spawnedPuppet.transform.Find("PlayerHead");
                if (headTransform != null)
                {
                    Image headImage = headTransform.GetComponent<Image>();
                    if (headImage != null && GameManager.Instance != null && GameManager.Instance.playerCustomizations.Count > pData.playerID)
                    {
                        int colorIndex = GameManager.Instance.playerCustomizations[pData.playerID].colorIndex;
                        if (colorIndex >= 0 && colorIndex < colorIcons.Length)
                        {
                            headImage.sprite = colorIcons[colorIndex];
                        }
                    }
                }

                // Trigger the puppet sliding up animation
                Animator puppetAnim = spawnedPuppet.GetComponent<Animator>();
                if (puppetAnim != null) puppetAnim.Play("PuppetSlideUp");
            }

            // Play the reaction sound (Cheer or Sad Trombone)
            if (audioSource != null && reactionClip != null)
            {
                audioSource.PlayOneShot(reactionClip);
            }

            // Wait 2 seconds so players can see who popped up before the next drumroll starts
            yield return new WaitForSeconds(2.0f);
        }
    }
}