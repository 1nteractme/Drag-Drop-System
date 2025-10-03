using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    private int _width;
    private int _height;

    private InventoryItem[,] _items;
    private static InventorySystem _instance;

    public static InventorySystem Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindAnyObjectByType<InventorySystem>();

            return _instance;
        }
    }

    public event Action<InventoryItem, int, int> OnItemAdded;
    public event Action<int, int> OnItemRemoved;
    public event Action<int, int, int, int> OnItemMoved;
    public event Action OnInventoryCleared;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            InitializeInventory();
        }
        else if (_instance != this)
            Destroy(gameObject);
    }

    private void InitializeInventory()
    {
        var ic = FindAnyObjectByType<InventoryGridController>();
        _width = ic.GridWidth;
        _height = ic.GridHeight;

        if (ic == null) return;
        _items = new InventoryItem[_width, _height];
    }

    public bool AddItem(ItemData itemData, int quantity = 1)
    {
        if (itemData == null) return false;
        if (quantity <= 0) return false;
        if (itemData.maxStackSize > 1 && ToStacks(itemData, quantity) <= 0) return true;

        return ToSlots(itemData, quantity);
    }

    private int ToStacks(ItemData itemData, int quantity)
    {
        for (var x = 0; x < _width; x++)
        {
            for (var y = 0; y < _height; y++)
            {
                var i = _items[x, y];

                if (i != null && i.data == itemData && i.stackSize < itemData.maxStackSize)
                {
                    var spaceLeft = i.GetSpaceLeft();
                    var amountToAdd = Mathf.Min(spaceLeft, quantity);

                    i.stackSize += amountToAdd;
                    quantity -= amountToAdd;

                    OnItemAdded?.Invoke(i, x, y);

                    if (quantity <= 0) return 0;
                }
            }
        }

        return quantity;
    }

    private bool ToSlots(ItemData itemData, int quantity)
    {
        for (var x = 0; x < _width; x++)
        {
            for (var y = 0; y < _height; y++)
            {
                if (_items[x, y] == null)
                {
                    var amountToAdd = Mathf.Min(itemData.maxStackSize, quantity);
                    var i = new InventoryItem(itemData, amountToAdd);
                    _items[x, y] = i;
                    quantity -= amountToAdd;

                    OnItemAdded?.Invoke(i, x, y);

                    if (quantity <= 0) return true;
                }
            }
        }

        Debug.LogWarning("Not enough space in inventory!");
        return false;
    }

    public bool MoveItem(int fromX, int fromY, int toX, int toY)
    {
        if (!IsValidSlot(fromX, fromY) || !IsValidSlot(toX, toY)) return false;

        var fromItem = _items[fromX, fromY];
        var toItem = _items[toX, toY];

        if (fromItem == null) return false;

        if (toItem == null)
        {
            _items[toX, toY] = fromItem;
            _items[fromX, fromY] = null;

            OnItemMoved?.Invoke(fromX, fromY, toX, toY);
            return true;
        }

        if (fromItem.data == toItem.data && fromItem.IsStackable())
        {
            var spaceLeft = toItem.GetSpaceLeft();
            if (spaceLeft > 0)
            {
                var amountToTransfer = Mathf.Min(spaceLeft, fromItem.stackSize);
                toItem.stackSize += amountToTransfer;
                fromItem.stackSize -= amountToTransfer;

                if (fromItem.stackSize <= 0)
                {
                    _items[fromX, fromY] = null;
                    OnItemRemoved?.Invoke(fromX, fromY);
                }
                else
                    OnItemAdded?.Invoke(fromItem, fromX, fromY);

                OnItemAdded?.Invoke(toItem, toX, toY);
                return true;
            }
        }

        _items[fromX, fromY] = toItem;
        _items[toX, toY] = fromItem;

        OnItemMoved?.Invoke(fromX, fromY, toX, toY);
        return true;
    }

    public void ClearInventory()
    {
        if (_items == null)
        {
            Debug.LogWarning("Inventory is not initialized!");
            return;
        }

        bool hadItems = false;

        for (var x = 0; x < _width; x++)
        {
            for (var y = 0; y < _height; y++)
            {
                if (_items[x, y] != null)
                {
                    _items[x, y] = null;
                    hadItems = true;
                }
            }
        }

        if (hadItems)
        {
            OnInventoryCleared?.Invoke();
            Debug.Log("All items removed from inventory");
        }
        else
            Debug.Log("Inventory was already empty");
    }

    public void RemoveItem(int x, int y, int quantity = 1)
    {
        if (!IsValidSlot(x, y) || _items[x, y] == null) return;

        var item = _items[x, y];

        if (quantity >= item.stackSize)
        {
            _items[x, y] = null;
            OnItemRemoved?.Invoke(x, y);
        }
        else
        {
            item.stackSize -= quantity;
            OnItemAdded?.Invoke(item, x, y);
        }
    }

    public bool UseItem(int x, int y, bool canUse)
    {
        if (!IsValidSlot(x, y) || _items[x, y] == null || !_items[x, y].data.isUsable)
            return false;

        if (canUse)
        {
            var itemName = _items[x, y].data.itemName;
            RemoveItem(x, y, 1);
            Debug.Log($"Using {itemName}");
        }

        return true;
    }

    public InventoryItem GetItem(int x, int y)
    {
        if (!IsValidSlot(x, y))
            return null;

        if (_items == null)
            return null;

        return _items[x, y];
    }

    private bool IsValidSlot(int x, int y)
    {
        if (_items == null)
            return false;

        return x >= 0 && x < _width && y >= 0 && y < _height;
    }

    public bool IsInitialized() => _items != null;

    #region Save/Load System

    [System.Serializable]
    private class SaveData
    {
        public List<SlotData> slots = new List<SlotData>();
    }

    [System.Serializable]
    private class SlotData
    {
        public int x, y;
        public string itemName;
        public int stackSize;
    }

    public void SaveInventory()
    {
        if (_items == null) return;

        var saveData = new SaveData();

        for (var x = 0; x < _width; x++)
        {
            for (var y = 0; y < _height; y++)
            {
                if (_items[x, y] != null)
                {
                    saveData.slots.Add(new SlotData
                    {
                        x = x,
                        y = y,
                        itemName = _items[x, y].data.name,
                        stackSize = _items[x, y].stackSize
                    });
                }
            }
        }

        var json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("InventoryData", json);
        PlayerPrefs.Save();
        
        Debug.Log($"Inventory was succesfuly saved with data:\n{json}");
    }

    private void OnApplicationQuit() => SaveInventory();

    #endregion
}