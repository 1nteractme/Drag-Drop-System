using UnityEngine;

public class InventoryGridController : MonoBehaviour
{
    [Header("Grid Settings")] 
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform gridParent;
    [SerializeField] private int gridWidth;
    [SerializeField] private int gridHeight;

    private InventorySlotUI[,] _slots;
    
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;

    private void Start() => Invoke(nameof(InitializeGrid), 0.1f);

    private void InitializeGrid()
    {
        if (InventorySystem.Instance == null)
        {
            Debug.LogError("InventorySystem is not available!");
            return;
        }

        if (!InventorySystem.Instance.IsInitialized())
        {
            Debug.LogError("InventorySystem is not initialized!");
            return;
        }

        _slots = new InventorySlotUI[gridWidth, gridHeight];

        for (var y = 0; y < gridHeight; y++)
        {
            for (var x = 0; x < gridWidth; x++)
            {
                var slot = Instantiate(slotPrefab, gridParent).GetComponent<InventorySlotUI>();

                if (slot != null)
                {
                    slot.Initialize(x, y);
                    _slots[x, y] = slot;
                }
                else
                    Debug.LogError($"Failed to get InventorySlotUI component at ({x},{y})");
            }
        }

        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnItemAdded += OnItemAdded;
            InventorySystem.Instance.OnItemRemoved += OnItemRemoved;
            InventorySystem.Instance.OnItemMoved += OnItemMoved;
            InventorySystem.Instance.OnInventoryCleared += OnInventoryCleared;
        }
    }

    private void OnItemAdded(InventoryItem item, int x, int y) => UpdateSlotUI(x, y);

    private void OnItemRemoved(int x, int y) => UpdateSlotUI(x, y);

    private void OnItemMoved(int fromX, int fromY, int toX, int toY)
    {
        UpdateSlotUI(fromX, fromY);
        UpdateSlotUI(toX, toY);
    }

    private void OnInventoryCleared()
    {
        for (var x = 0; x < gridWidth; x++)
        {
            for (var y = 0; y < gridHeight; y++)
            {
                if (IsValidSlot(x, y))
                    _slots[x, y].UpdateUI();
            }
        }
    }

    private void UpdateSlotUI(int x, int y)
    {
        if (IsValidSlot(x, y))
            _slots[x, y].UpdateUI();
    }

    private bool IsValidSlot(int x, int y) => x >= 0 && x < gridWidth && y >= 0 && y < gridHeight && _slots != null;

    private void OnDestroy()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnItemAdded -= OnItemAdded;
            InventorySystem.Instance.OnItemRemoved -= OnItemRemoved;
            InventorySystem.Instance.OnItemMoved -= OnItemMoved;
            InventorySystem.Instance.OnInventoryCleared -= OnInventoryCleared;
        }
    }
}