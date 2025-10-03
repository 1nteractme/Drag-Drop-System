using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory System/Item")]
public class ItemData : ScriptableObject
{
    // Main Task
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public ItemType type;

    // Additional
    public int maxStackSize = 1;
    public bool isUsable = false;
}