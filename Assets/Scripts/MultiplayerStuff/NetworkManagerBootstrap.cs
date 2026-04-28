using UnityEngine;

public class NetworkManagerBootstrap : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}