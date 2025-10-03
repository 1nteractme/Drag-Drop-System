using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class InventorySlotUI : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IBeginDragHandler
{
    [Header("UI Components")] 
    
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI stackText;
    [SerializeField] private GameObject selectedHighlight;

    private int _x, _y;
    private bool _isInit;
    private CanvasGroup _cg;
    private RectTransform _rt;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _cg = GetComponent<CanvasGroup>();
    }

    public void Initialize(int x, int y)
    {
        this._x = x;
        this._y = y;

        _isInit = InventorySystem.Instance != null &&
                  InventorySystem.Instance.IsInitialized() &&
                  TooltipSystem.Instance != null;

        UpdateUI();
        SetSelected(false);
    }

    public void UpdateUI()
    {
        if (!_isInit)
        {
            ClearSlot();
            return;
        }

        var item = InventorySystem.Instance.GetItem(_x, _y);

        if (item != null && item.data != null)
            SetActiveSlot(item);
        else
            ClearSlot();
    }

    private void ClearSlot()
    {
        itemIcon.enabled = false;
        stackText.text = "";
    }

    private void SetActiveSlot(InventoryItem item)
    {
        itemIcon.enabled = true;
        itemIcon.sprite = item.data.icon;
        stackText.text = item.stackSize > 1 ? $"x{item.stackSize.ToString()}" : "";
    }

    private void SetSelected(bool selected)
    {
        if (selectedHighlight != null)
            selectedHighlight.SetActive(selected);
    }

    #region Drag & Drop

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_isInit || InventorySystem.Instance.GetItem(_x, _y) == null)
        {
            eventData.pointerDrag = null;
            return;
        }

        if (_cg != null)
        {
            _cg.alpha = 0.6f;
            _cg.blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isInit) return;

        if (_rt != null)
        {
            var canvas = GetComponentInParent<Canvas>();

            if (canvas != null)
                _rt.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_cg != null)
        {
            _cg.alpha = 1f;
            _cg.blocksRaycasts = true;
        }

        if (_rt != null)
            _rt.anchoredPosition = Vector2.zero;

        if (!_isInit || InventorySystem.Instance == null) return;

        if (eventData.hovered != null)
        {
            foreach (var raycast in eventData.hovered)
            {
                if (raycast == null) continue;
                var target = raycast.GetComponent<InventorySlotUI>();

                if (target != null && target != this && target._isInit)
                {
                    InventorySystem.Instance.MoveItem(_x, _y, target._x, target._y);
                    break;
                }
            }
        }

        SetSelected(false);
        TooltipSystem.Instance.HideTooltip();
    }

    #endregion

    #region Interaction

    private void OnItemClicked()
    {
        if (!_isInit) return;

        var item = InventorySystem.Instance.GetItem(_x, _y);
        if (item == null || item.data == null)
        {
            TooltipSystem.Instance.HideTooltip();
            return;
        }

        TooltipSystem.Instance.ShowTooltip(item.data.itemName, item.data.description, item.data.icon, item.stackSize, _x, _y);
        SetSelected(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_isInit) return;
        OnItemClicked();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isInit) return;
        SetSelected(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isInit) return;
        SetSelected(false);
    }

    #endregion
}