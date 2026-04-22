using System.Collections;
using UnityEngine;

public class SprinklerBehaviour : MonoBehaviour
{
    [Header("VFX")]
    public GameObject sprinklerEffect;
    public GameObject rippleEffect;

    private bool ripplePlayed = false;

    public void Activate()
    {
        StartCoroutine(RunSprinkler());
    }

    private IEnumerator RunSprinkler()
    {
        ripplePlayed = false;

        // turn on water spray
        if (sprinklerEffect != null)
            sprinklerEffect.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        // burst ripple ONCE
        if (rippleEffect != null && !ripplePlayed)
        {
            ripplePlayed = true;

            rippleEffect.SetActive(true);

            ParticleSystem ps = rippleEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play(true);
            }
        }

        yield return new WaitForSeconds(12f);

        // turn ripple off
        if (rippleEffect != null)
            rippleEffect.SetActive(false);

        // keep spray going while active (optional)
    }

    void OnParticleCollision(GameObject other)
    {
        Debug.Log("Ripple hit: " + other.name);
    }

}