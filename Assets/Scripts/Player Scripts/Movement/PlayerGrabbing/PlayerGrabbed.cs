using UnityEngine;

public class PlayerGrabbed : MonoBehaviour
{
    [HideInInspector]
    public PlayerGrab grabber;

    public void ReleaseGrabbedPlayer()
    {
        if (grabber != null)
        {
            Playermovement movement = grabber.GetComponent<Playermovement>();
            if (movement != null)
            {
                Debug.Log($"Grabber index {movement.playerIndex} released their grabbed player.");
            }

            grabber.ReleaseGrab();
        }
    }
}
