[System.Serializable]
public class InventoryItem
{
    public ItemData data;
    public int stackSize;
    public string itemId;

    public InventoryItem(ItemData data, int stackSize = 1)
    {
        this.data = data;
        this.stackSize = stackSize;
        this.itemId = System.Guid.NewGuid().ToString();
    }

    public bool CanStackWith(InventoryItem other) =>
        other != null && data == other.data && stackSize < data.maxStackSize;

    public int GetSpaceLeft() => data.maxStackSize - stackSize;

    public bool IsStackable() => data.maxStackSize > 1;
}