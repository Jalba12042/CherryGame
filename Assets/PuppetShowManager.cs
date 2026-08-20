using UnityEngine;
using TMPro;

public class PuppetShowManager : MonoBehaviour
{
    [Header("Puppet Show Groups (Parents)")]
    public GameObject parkShow;
    public GameObject beachShow;
    public GameObject mountainShow;

    [Header("UI Text Elements")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI tipsText;

    // We use OnEnable so this checks the upcoming level every single time this screen appears!
    void OnEnable()
    {
        // 1. Turn all shows OFF by default so they don't overlap
        if (parkShow != null) parkShow.SetActive(false);
        if (beachShow != null) beachShow.SetActive(false);
        if (mountainShow != null) mountainShow.SetActive(false);

        // 2. Ask the RoundManager what round was selected
        if (RoundManager.Instance != null && RoundManager.Instance.currRound != null)
        {
            string nextScene = RoundManager.Instance.currRound.sceneName;

            // 3. Turn on the correct show and update the cardboard text!
            // IMPORTANT: Make sure these names exactly match your Scene names in Unity!

            if (nextScene == "Park" || nextScene == "ParkTest") // Change string to your exact Park scene name
            {
                if (parkShow != null) parkShow.SetActive(true);
                if (levelText != null) levelText.text = "Level Loading: Park";
                if (tipsText != null) tipsText.text = "Watch out for the meteor!";
            }
            else if (nextScene == "Beach" || nextScene == "BeachTest") // Change string to your exact Beach scene name
            {
                if (beachShow != null) beachShow.SetActive(true);
                if (levelText != null) levelText.text = "Level Loading: Beach";
                if (tipsText != null) tipsText.text = "Tip: High tides will drag you into the water!";
            }
            else if (nextScene == "Mountain" || nextScene == "Iceberg") // Change string to your exact Mountain scene name
            {
                if (mountainShow != null) mountainShow.SetActive(true);
                if (levelText != null) levelText.text = "Level Loading: Mountain";
                if (tipsText != null) tipsText.text = "Did you know? Snowballs deal extra knockback!";
            }
        }
    }
}