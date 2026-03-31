using UnityEngine;

public class FaceCamManager : MonoBehaviour
{
    public static FaceCamManager Instance;

    public FaceCamStatic[] faceCamStatics; // assign P1–P4 FaceCamStatic here

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public FaceCamStatic GetFaceCamStatic(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < faceCamStatics.Length)
            return faceCamStatics[playerIndex];
        return null;
    }
}