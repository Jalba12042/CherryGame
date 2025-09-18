using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int playerCount;
    public int[] controllerAssignments;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}