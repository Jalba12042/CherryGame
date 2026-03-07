using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuMusic : MonoBehaviour
{
    private static MainMenuMusic instance;
    private AudioSource audioSource;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    void Awake()
    {
        // 1. The Singleton Pattern: Ensures only ONE music manager ever exists.
        // If we load back into the Title Screen, it destroys the duplicate.
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject); // Keeps this object alive between scenes

        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        // Subscribe to the scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Unsubscribe when disabled to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if the scene we just loaded into is your Loading Screen
        // IMPORTANT: Make sure this string exactly matches your loading scene's name!
        if (scene.name == "RossTestScene")
        {
            StartCoroutine(FadeOut());
        }
    }

    IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;

        // Gradually reduce volume to 0
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume; // Reset volume in case we go back to the main menu later
    }
}
