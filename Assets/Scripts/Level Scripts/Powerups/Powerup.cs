using System.Collections;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField] protected float duration;
    [SerializeField] protected string puName;

    protected Playermovement pc;
    protected GameObject playerModel;

    public void Activate(Playermovement player)
    {
        pc = player;
        playerModel = player.gameObject;
        StartCoroutine(StartTimer());
    }

    protected virtual IEnumerator StartTimer()
    {
        if (pc == null) yield break;

        // start effect
        powerUpEffect();

        // visually hide object
        GetComponent<Collider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;

        yield return new WaitForSeconds(duration);

        powerUpEnd();
        //Destroy(gameObject);
    }

    //RoundManager.Instance.currRound.powerupsInPlay.Remove(gameObject);

    protected virtual void powerUpEffect()
    {
        Debug.Log($"Powerup activated: {puName} for {pc.name}");
    }

    protected virtual void powerUpEnd()
    {
        Debug.Log($"Powerup ended: {puName} for {pc.name}");
    }
}
