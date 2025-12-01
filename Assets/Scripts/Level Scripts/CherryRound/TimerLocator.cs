using TMPro;
using UnityEngine;

public class TimerLocator : MonoBehaviour
{
    private void Awake()
    {
        RoundManager.Instance?.SetTimer(GetComponent<TextMeshProUGUI>());
    }
}
