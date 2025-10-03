using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem Instance { get; private set; }

    [SerializeField] private GameObject tooltipPanel;

    [Header("Tooltip UI Components")] 
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI stackSizeText;
    [SerializeField] private Image itemIcon;

    [Space] 
    [Header("Tooltip UI Buttons")] 
    [SerializeField] private Button saveButton;
    [SerializeField] private Button dropButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button useButton;

    private int _x = -1, _y = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            HideTooltip();
            InitButtons();
        }
        else
            Destroy(gameObject);
    }

    private void InitButtons()
    {
        if (saveButton != null)
        {
            saveButton.onClick.RemoveAllListeners();
            saveButton.onClick.AddListener(SaveButtonClicked);
        }

        if (dropButton != null)
        {
            dropButton.onClick.RemoveAllListeners();
            dropButton.onClick.AddListener(OnDropButtonClicked);
        }

        if (clearButton != null)
        {
            clearButton.onClick.RemoveAllListeners();
            clearButton.onClick.AddListener(OnClearButtonClicked);
        }

        if (useButton != null)
        {
            useButton.onClick.RemoveAllListeners();
            useButton.onClick.AddListener(OnUseButtonClicked);
        }
    }

    private void SaveButtonClicked()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.SaveInventory();
    }

    private void OnDropButtonClicked()
    {
        if (InventorySystem.Instance != null && _x >= 0 && _y >= 0)
        {
            InventorySystem.Instance.RemoveItem(_x, _y);
            HideTooltip();
        }
    }

    private void OnClearButtonClicked()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.ClearInventory();
            HideTooltip();
        }
    }

    private void OnUseButtonClicked()
    {
        if (InventorySystem.Instance != null && _x >= 0 && _y >= 0)
        {
            InventorySystem.Instance.UseItem(_x, _y, true);
            HideTooltip();
        }
    }

    public void ShowTooltip(string title, string description, Sprite icon, int stackSize, int x, int y)
    {
        titleText.text = title;
        descriptionText.text = description;
        stackSizeText.text = $"x{stackSize}";
        itemIcon.sprite = icon;

        _x = x;
        _y = y;
        
        useButton.interactable = InventorySystem.Instance.UseItem(_x, _y, false);;

        tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
        _x = -1;
        _y = -1;
    }
}