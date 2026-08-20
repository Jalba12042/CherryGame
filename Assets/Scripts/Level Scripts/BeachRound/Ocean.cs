using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Tide
{
    RollingInSpawn,
    RollingOutDrag,
    RollingIn,
    RollingOut
}

public class Ocean : MonoBehaviour
{
    [Header("Positions")]
    public Transform startPoint;
    public Transform inPoint;

    [Header("Timing")]
    public float rollInDuration = 3f;
    public float stayDuration = 5f;
    public float rollOutDuration = 3f;
    public float waveWaitDuration = 5f;

    // ==========================================
    // --- NEW: Audio Settings ---
    // ==========================================
    [Header("Audio Settings")]
    public AudioSource rollInAudio;
    public AudioSource rollOutAudio;
    [Tooltip("How fast the sound fades out when the wave finishes moving")]
    public float fadeOutSpeed = 1.0f;

    [Header("Shell Spawning")]
    [SerializeField] private GameObject[] shellPrefabs;
    [SerializeField] private int minSpawns = 1;
    [SerializeField] private int maxSpawns = 5;
    public List<GameObject> currShells;

    [Header("Drag Settings")]
    public float dragStrength = 1f;
    public LayerMask draggableLayers;

    public bool tideRolling = true;

    private List<Rigidbody> caughtObjects = new List<Rigidbody>();
    private Vector3 lastPosition;
    private bool roundStarted = false;

    private void Update()
    {
        if (RoundManager.Instance.currRoundActive && !roundStarted)
        {
            roundStarted = true;
            StartCoroutine(TideCycle());
        }
    }

    public IEnumerator TideCycle()
    {
        Bounds b = GetComponent<Collider>().bounds;
        while (RoundManager.Instance.currRoundActive)
        {
            // --- WAVE PHASE 1: ROLL IN ---
            if (rollInAudio != null) { rollInAudio.volume = 1f; rollInAudio.Play(); }

            int randSpawns = Random.Range(minSpawns, maxSpawns + 1);
            for (int i = 0; i < randSpawns; i++)
            {
                float randX = Random.Range(b.min.x, b.max.x);
                float randZ = Random.Range(b.min.z, b.max.z);
                int randShellIndex = Random.Range(0, shellPrefabs.Length);
                currShells.Add(Instantiate(shellPrefabs[randShellIndex], new Vector3(randX, transform.position.y, randZ), Quaternion.identity));
            }

            float t = 0;
            lastPosition = transform.position;
            while (t < rollInDuration)
            {
                t += Time.deltaTime;
                Vector3 newPosition = Vector3.Lerp(startPoint.position, inPoint.position, Mathf.SmoothStep(0, 1, t / rollInDuration));
                Vector3 tideMovement = newPosition - lastPosition;
                lastPosition = newPosition;
                transform.position = newPosition;

                foreach (Rigidbody rb in caughtObjects)
                {
                    if (rb == null) continue;

                    // Someone else may have killed/respawned this player while the wave still had
                    // them — without this check we keep dragging their (now kinematic, already
                    // spawn-positioned) rigidbody away from their spawn point while they're invisible.
                    PlayerKill deadCheck = rb.GetComponent<PlayerKill>();
                    if (deadCheck != null && deadCheck.currDead) continue;

                    rb.MovePosition(rb.position + tideMovement * dragStrength);
                }
                yield return null;
            }

            // --- WAVE PHASE 1: STAY ---
            // Fade out the "Roll In" audio smoothly during the stay duration
            if (rollInAudio != null) StartCoroutine(FadeOutAudio(rollInAudio, stayDuration));
            yield return new WaitForSeconds(stayDuration);

            // --- WAVE PHASE 1: ROLL OUT ---
            // Instantly start the "Roll Out" audio at full volume!
            if (rollOutAudio != null) { rollOutAudio.volume = 1f; rollOutAudio.Play(); }

            t = 0;
            lastPosition = transform.position;
            while (t < rollOutDuration)
            {
                t += Time.deltaTime;
                Vector3 newPosition = Vector3.Lerp(inPoint.position, startPoint.position, Mathf.SmoothStep(0, 1, t / rollOutDuration));
                lastPosition = newPosition;
                transform.position = newPosition;
                yield return null;
            }

            // --- WAVE PHASE 1: WAIT ---
            // Fade out the "Roll Out" audio smoothly
            if (rollOutAudio != null) StartCoroutine(FadeOutAudio(rollOutAudio, fadeOutSpeed));
            yield return new WaitForSeconds(waveWaitDuration);


            // --- WAVE PHASE 2: ROLL IN ---
            if (rollInAudio != null) { rollInAudio.volume = 1f; rollInAudio.Play(); }

            t = 0;
            lastPosition = transform.position;
            while (t < rollInDuration)
            {
                t += Time.deltaTime;
                Vector3 newPosition = Vector3.Lerp(startPoint.position, inPoint.position, Mathf.SmoothStep(0, 1, t / rollInDuration));
                lastPosition = newPosition;
                transform.position = newPosition;
                yield return null;
            }

            // Since Wave 2 doesn't have a "Stay" duration, we fade out rapidly over 0.5 seconds
            if (rollInAudio != null) StartCoroutine(FadeOutAudio(rollInAudio, 0.5f));

            // --- WAVE PHASE 2: ROLL OUT (With Drag) ---
            if (rollOutAudio != null) { rollOutAudio.volume = 1f; rollOutAudio.Play(); }

            t = 0;
            lastPosition = transform.position;
            while (t < rollOutDuration)
            {
                t += Time.deltaTime;
                Vector3 newPosition = Vector3.Lerp(inPoint.position, startPoint.position, Mathf.SmoothStep(0, 1, t / rollOutDuration));
                Vector3 tideMovement = newPosition - lastPosition;
                lastPosition = newPosition;
                transform.position = newPosition;

                foreach (Rigidbody rb in caughtObjects)
                {
                    if (rb == null) continue;

                    // Same as the roll-in drag loop above — don't keep dragging a player who was
                    // already killed/respawned elsewhere while still caught by the wave.
                    PlayerKill deadCheck = rb.GetComponent<PlayerKill>();
                    if (deadCheck != null && deadCheck.currDead) continue;

                    Playermovement pm = rb.GetComponent<Playermovement>();
                    if (pm != null) pm.canMove = false;
                    rb.MovePosition(rb.position + tideMovement * dragStrength);
                }
                yield return null;
            }

            for (int i = caughtObjects.Count - 1; i >= 0; i--)
            {
                if (caughtObjects[i] == null) continue;
                PlayerKill pk = caughtObjects[i].GetComponent<PlayerKill>();
                Playermovement pm = caughtObjects[i].GetComponent<Playermovement>();
                if (pk != null && pm != null)
                {
                    if (pk.currDead) continue;
                    pm.canMove = true;
                    pk.killPlayer(true);
                }
                else
                    Destroy(caughtObjects[i].gameObject);
            }
            caughtObjects.Clear();

            // --- WAVE PHASE 2: WAIT ---
            if (rollOutAudio != null) StartCoroutine(FadeOutAudio(rollOutAudio, fadeOutSpeed));
            yield return new WaitForSeconds(waveWaitDuration);
        }
    }

    // ==========================================
    // --- NEW: Audio Crossfading Coroutine ---
    // ==========================================
    private IEnumerator FadeOutAudio(AudioSource audioSource, float fadeTime)
    {
        if (audioSource == null || !audioSource.isPlaying) yield break;

        float startVolume = audioSource.volume;
        float time = 0f;

        while (time < fadeTime)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeTime);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume; // Reset volume back to 100% so it's ready for the next wave
    }

    private void OnTriggerEnter(Collider other)
    {
        // Zombies are always caught regardless of layer (they're on Default, not draggableLayers),
        // so they get dragged and killed by the wave the same way players do.
        bool isDraggableLayer = ((1 << other.gameObject.layer) & draggableLayers) != 0;
        bool isZombie = other.GetComponent<Zombie>() != null;
        if (!isDraggableLayer && !isZombie) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && !caughtObjects.Contains(rb))
        {
            caughtObjects.Add(rb);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            caughtObjects.Remove(rb);
        }

        Playermovement pm = other.GetComponent<Playermovement>();
        if (pm != null)
        {
            pm.canMove = true;
        }
    }
}