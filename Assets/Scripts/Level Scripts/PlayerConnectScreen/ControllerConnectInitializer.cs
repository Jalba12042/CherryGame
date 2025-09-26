using UnityEngine;

public class ControllerConnectInitializer : MonoBehaviour
{
    // References to the controller connection GameObjects for each player count.
    // These should be assigned in the Inspector and correspond to the UI setups
    // for 2-player, 3-player, and 4-player configurations.
    public GameObject twoPlayerConnect;
    public GameObject threePlayerConnect;
    public GameObject fourPlayerConnect;

    void Start()
    {
        // Retrieve the number of players selected from the title screen.
        int count = GameManager.Instance.playerCount;


        // Activate the correct controller connection object based on player count.
        // Only one of these should be active at a time.
        twoPlayerConnect.SetActive(count == 2);
        threePlayerConnect.SetActive(count == 3);
        fourPlayerConnect.SetActive(count == 4);
    }
}
