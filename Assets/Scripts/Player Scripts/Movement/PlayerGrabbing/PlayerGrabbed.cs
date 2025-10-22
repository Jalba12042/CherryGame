using UnityEngine;

public class PlayerGrabbed : MonoBehaviour
{
    [HideInInspector]
    public Playermovement grabber;

    public void ReleaseGrabbedPlayer()
    {
        if (grabber != null)
        {
            Debug.Log($"Grabber index {grabber.playerIndex} released their grabbed player.");
            grabber.HandlePlayerRelease();
        }
    }
}
