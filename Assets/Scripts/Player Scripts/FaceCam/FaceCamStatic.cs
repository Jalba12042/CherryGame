using UnityEngine;

public class FaceCamStatic : MonoBehaviour
{
    public Animator anim;

    public void Play()
    {
        anim.gameObject.SetActive(true);  // enable the image
        anim.Play("Static", -1, 0f);  // play from start
    }

    public void Stop()
    {
        anim.gameObject.SetActive(false); // hide image
    }
}