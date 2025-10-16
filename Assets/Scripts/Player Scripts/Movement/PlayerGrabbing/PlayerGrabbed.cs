using UnityEngine;

public class PlayerGrabbed : MonoBehaviour
{
    [HideInInspector]
    public PlayerPickup grabber;

    public void ReleaseGrabbedPlayer()
    {
        if (grabber != null)
            grabber.ReleaseCurrentGrabbedPlayer();
    }
}
