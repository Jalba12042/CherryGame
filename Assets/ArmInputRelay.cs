using UnityEngine;

public class ArmInputRelay : MonoBehaviour
{
    [Tooltip("Drag your IntroManager object here!")]
    public AnyInputToAnimatorTrigger mainManager;

    // The animation will call this, and this will tell the Boss to arm the input!
    public void ArmInput()
    {
        if (mainManager != null)
        {
            mainManager.ArmInput();
        }
    }
}