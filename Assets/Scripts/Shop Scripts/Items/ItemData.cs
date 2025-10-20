using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item", order = 1)]
public class ItemData : ScriptableObject
{
    public string desc;
    public string itemName;
    public GameObject powerup;
}
