using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;

    [Header("Assign in Inspector")]
    public Canvas TransitionCanvas;     // e.g., Canvas_Transition (overlay UI canvas)
    public VideoPlayer VideoPlayer;     // The VideoPlayer that plays your paper fold video

    [Tooltip("Optional default scene if you call Go() with no args")]
    public string DefaultNextScene = "";

    [Header("Optional")]
    [Tooltip("If true, ignore video length and use a fixed duration instead")]
    public bool UseFixedDuration = false;

    [Tooltip("Used if UseFixedDuration is true, seconds")]
    public float FixedDuration = 1.5f;

    private bool _videoFinished;

    private void Awake()
    {
        // Singleton + persist
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (TransitionCanvas != null)
            {
                // Persist the overlay so references don't break after scene load
                DontDestroyOnLoad(TransitionCanvas.gameObject);
                TransitionCanvas.gameObject.SetActive(false);
            }

            if (VideoPlayer != null)
            {
                // Make sure VP won't auto-play on scene start
                VideoPlayer.playOnAwake = false;
                VideoPlayer.isLooping = false;
                VideoPlayer.skipOnDrop = true;
#if UNITY_2021_3_OR_NEWER
                VideoPlayer.waitForFirstFrame = true;
#endif
                // When the video ends, mark finished
                VideoPlayer.loopPointReached += _ => _videoFinished = true;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Hook this from your Button with a string parameter (scene name)
    public void Go(string sceneName)
    {
        StartCoroutine(PlayAndLoad(sceneName));
    }

    // Overload: use the default scene set in the inspector
    public void Go()
    {
        StartCoroutine(PlayAndLoad(DefaultNextScene));
    }

    private IEnumerator PlayAndLoad(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneTransition] Empty scene name.");
            yield break;
        }

        // Check the scene is in Build Settings or provided by an AssetBundle
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[SceneTransition] Scene \"{sceneName}\" is not in Build Settings or not available to load.");
            yield break;
        }

        // Show overlay
        if (TransitionCanvas != null)
            TransitionCanvas.gameObject.SetActive(true);

        // Prepare & play the video
        _videoFinished = false;
        if (VideoPlayer != null)
        {
            VideoPlayer.time = 0;
            VideoPlayer.Prepare();
            while (!VideoPlayer.isPrepared)
                yield return null;

            VideoPlayer.Play();
        }

        // Begin loading the next scene in the background
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null)
        {
            Debug.LogError($"[SceneTransition] LoadSceneAsync returned null for \"{sceneName}\".");
            yield break;
        }
        op.allowSceneActivation = false;

        // Optionally wait until the scene load is basically done (0.9f)
        while (op.progress < 0.9f)
            yield return null;

        // Wait for the video to finish OR a fixed duration
        if (UseFixedDuration)
        {
            yield return new WaitForSeconds(FixedDuration);
        }
        else if (VideoPlayer != null)
        {
            // Use actual video end
            double length = VideoPlayer.length;
            // Safety: if length is unknown, fall back to 1s
            if (double.IsNaN(length) || length <= 0.01)
                length = 1.0;

            while (!_videoFinished && VideoPlayer.time < length - 0.02f)
                yield return null;
        }
        else
        {
            // No video assigned; brief delay so the overlay is visible
            yield return new WaitForSeconds(0.5f);
        }

        // Activate the new scene right as the fold finishes
        op.allowSceneActivation = true;

        // Give one frame for the new scene to present
        yield return null;

        // Hide overlay (it persists between scenes, so guard for null)
        if (TransitionCanvas != null)
            TransitionCanvas.gameObject.SetActive(false);
    }
}
