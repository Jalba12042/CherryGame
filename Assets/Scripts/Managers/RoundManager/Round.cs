using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Round", menuName = "Rounds/Round", order = 1)]
public class Round : ScriptableObject
{
    public float roundTimeInSeconds;
    public string sceneName;

    public virtual IEnumerator StartGoal()
    {
        yield return null;
    }
}
