using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public GameObject canvas2P;
    public GameObject canvas3P;
    public GameObject canvas4P;

    void Start()
    {
        int players = GameManager.Instance.playerCount;

        canvas2P.SetActive(players == 2);
        canvas3P.SetActive(players == 3);
        canvas4P.SetActive(players == 4);
    }
}
