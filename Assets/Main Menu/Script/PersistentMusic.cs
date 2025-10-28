using UnityEngine;

public class PersistentMusic : MonoBehaviour
{
    private static PersistentMusic instance;

    void Awake()
    {
        // If one already exists, destroy this duplicate
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Keep this object between scenes
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

