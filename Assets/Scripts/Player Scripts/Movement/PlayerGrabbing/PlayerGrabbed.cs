using UnityEngine;

public class PlayerGrabbed : MonoBehaviour
{
    [HideInInspector]
    public PlayerPickup grabber;

    public void ReleaseGrabbedPlayer()
    {
        if (grabber != null)
        {
            Debug.Log($"Grabber index {grabber.playerIndex} released their grabbed player.");
            grabber.ReleaseCurrentGrabbedPlayer();
        }
    }
}
