using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item", order = 1)]
public class ItemData : ScriptableObject
{
    public string itemName;
    public string desc;
    public GameObject powerup;
    public bool added;
}
