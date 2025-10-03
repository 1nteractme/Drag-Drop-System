using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    [Header("Test Items")] 
    [SerializeField] private ItemData[] testItems;

    [Space] 
    [Header("Item Settings")] 
    [SerializeField] private int itemID;
    [SerializeField] private int itemAmount;

    [Space]
    [Header("Debug Settings")] 
    [SerializeField] private KeyCode addItemKey;
    [SerializeField] private bool logDetailedInfo = true;
    
    private void Start()
    {
        if (InventorySystem.Instance == null)
        {
            Debug.LogError("InventorySystem is not initialized!");
            return;
        }

        if (testItems == null || testItems.Length == 0)
            Debug.LogWarning("No test items assigned in InventoryTester!");

        else
        {
            var validItems = 0;

            foreach (var item in testItems)
                if (item != null)
                    validItems++;

            Debug.Log($"InventoryTester ready with {validItems}/{testItems.Length} valid test items");
        }
    }

    private void Update()
    {
        if (InventorySystem.Instance == null) return;

        if (Input.GetKeyDown(addItemKey))
            AddItem(itemAmount, itemAmount, addItemKey);
    }

    private void AddItem(int itemIndex, int quantity, KeyCode key)
    {
        switch (key)
        {
            case KeyCode.S:
                InventorySystem.Instance.SaveInventory();
                break;
            
            case KeyCode.R:
                AddRandomItem();
                break;
            
            case KeyCode.A:
                AddTestItem(itemIndex, quantity);
                break;
            
        }
    }
    
    private void AddTestItem(int index, int quantity)
    {
        if (index < 0 || index >= testItems.Length || testItems[index] == null)return;

        var success = InventorySystem.Instance.AddItem(testItems[index], quantity);

        if (logDetailedInfo)
        {
            TooltipSystem.Instance.HideTooltip();
            
            if (success)
                Debug.Log($"Successfully added {quantity}x {testItems[index].itemName}");
            else
                Debug.LogWarning($"Failed to add {quantity}x {testItems[index].itemName} - inventory full?");
        }
    }

    private void AddRandomItem()
    {
        if (testItems.Length == 0)
        {
            Debug.LogError("No valid test items found in array!");
            return;
        }
        
        var randomItem = testItems[Random.Range(0, testItems.Length)];
        
        if (randomItem == null)
        {
            Debug.LogError("Random item is null!");
            return;
        }
        
        // var randomQuantity = randomItem.maxStackSize > 1 ? Random.Range(1, randomItem.maxStackSize + 1) : 1;
        var success = InventorySystem.Instance.AddItem(randomItem, 1);

        if (logDetailedInfo)
        {
            if (success)
                Debug.Log($"Added random item: {randomItem.itemName} x{1}");
            else
                Debug.LogWarning($"Failed to add random item: {randomItem.itemName} - inventory full?");
        }
    }
}