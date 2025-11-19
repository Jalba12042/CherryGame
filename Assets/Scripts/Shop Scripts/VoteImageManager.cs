using UnityEngine;
using UnityEngine.UI;

public class VoteImageManager : MonoBehaviour
{
    public GameObject[] playerOneVoteImages;
    public GameObject[] playerTwoVoteImages;
    public GameObject[] playerThreeVoteImages;
    public GameObject[] playerFourVoteImages;

    public int playerOneCurrVoteIndex;
    public int playerTwoCurrVoteIndex;
    public int playerThreeCurrVoteIndex;
    public int playerFourCurrVoteIndex;

    private void Start()
    {
        for (int i = 0; i < playerOneVoteImages.Length; i++)
        {
            playerOneVoteImages[i].SetActive(false);
            playerTwoVoteImages[i].SetActive(false);
            playerThreeVoteImages[i].SetActive(false);
            playerFourVoteImages[i].SetActive(false);
        }

        playerOneCurrVoteIndex = -1;
        playerTwoCurrVoteIndex = -1;
        playerThreeCurrVoteIndex = -1;
        playerFourCurrVoteIndex = -1;
    }

    public void changeVote(int player, int vote)
    {
        switch (player)
        {
            /*
             * 1. check if there is a vote, if there is, remove the image
             * 2. change the vote
             * 3. change the image
             */

            case 0:
                if (playerOneCurrVoteIndex != -1)
                {
                    playerOneVoteImages[playerOneCurrVoteIndex].SetActive(false);
                }

                playerOneCurrVoteIndex = vote;

                playerOneVoteImages[playerOneCurrVoteIndex].SetActive(true);
                break;
            case 1:
                if (playerTwoCurrVoteIndex != -1)
                {
                    playerTwoVoteImages[playerTwoCurrVoteIndex].SetActive(false);
                }

                playerTwoCurrVoteIndex = vote;

                playerTwoVoteImages[playerTwoCurrVoteIndex].SetActive(true);
                break;
            case 2:
                if (playerThreeCurrVoteIndex != -1)
                {
                    playerThreeVoteImages[playerThreeCurrVoteIndex].SetActive(false);
                }

                playerThreeCurrVoteIndex = vote;

                playerThreeVoteImages[playerThreeCurrVoteIndex].SetActive(true);
                break;
            case 3:
                if (playerFourCurrVoteIndex != -1)
                {
                    playerFourVoteImages[playerFourCurrVoteIndex].SetActive(false);
                }

                playerFourCurrVoteIndex = vote;

                playerFourVoteImages[playerFourCurrVoteIndex].SetActive(true);
                break;
        }
    }
}
