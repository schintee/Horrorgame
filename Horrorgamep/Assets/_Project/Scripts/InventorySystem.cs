using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    // Optional: notify other systems when inventory changes
    public System.Action OnInventoryChanged;

    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    [Header("Wrong Key Message")]
    [SerializeField] private GameObject wrongKeyCanvas;
    [SerializeField] private Text wrongKeyText;
    [SerializeField] private float wrongKeyDisplayTime = 2f;

    private readonly Dictionary<string, InventoryItem> items = new Dictionary<string, InventoryItem>();
    private readonly List<GameObject> itemSlots = new List<GameObject>();
    private bool isOpen;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        if (wrongKeyCanvas != null)
            wrongKeyCanvas.SetActive(false);

        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            ToggleInventory();
    }

    public void AddItem(string itemID, string itemName, Sprite itemIcon = null)
    {
        if (string.IsNullOrWhiteSpace(itemID)) return;

        if (items.TryGetValue(itemID, out var existing))
        {
            existing.quantity++;
            items[itemID] = existing;
        }
        else
        {
            items[itemID] = new InventoryItem
            {
                itemID = itemID,
                itemName = itemName,
                icon = itemIcon,
                quantity = 1
            };
        }

        UpdateUI();
        OnInventoryChanged?.Invoke();
    }

    public bool HasItem(string itemID)
    {
        return !string.IsNullOrWhiteSpace(itemID)
               && items.TryGetValue(itemID, out var it)
               && it.quantity > 0;
    }

    public void RemoveItem(string itemID, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(itemID)) return;
        if (amount <= 0) amount = 1;

        if (!items.TryGetValue(itemID, out var it)) return;

        it.quantity -= amount;

        if (it.quantity <= 0)
            items.Remove(itemID);
        else
            items[itemID] = it;

        UpdateUI();
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// IMPORTANT (used on enemy catch / reset): clears all items + refresh UI.
    /// EnemyBrain calls this.
    /// </summary>
    public void ClearInventory()
    {
        items.Clear();
        UpdateUI();
        OnInventoryChanged?.Invoke();
    }

    public void ShowWrongKeyMessage(string requiredKeyID)
    {
        if (wrongKeyCanvas == null) return;

        wrongKeyCanvas.SetActive(true);

        if (wrongKeyText != null)
            wrongKeyText.text = $"You need: {requiredKeyID}";

        CancelInvoke(nameof(HideWrongKeyMessage));
        Invoke(nameof(HideWrongKeyMessage), wrongKeyDisplayTime);
    }

    private void HideWrongKeyMessage()
    {
        if (wrongKeyCanvas != null)
            wrongKeyCanvas.SetActive(false);
    }

    private void ToggleInventory()
    {
        isOpen = !isOpen;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(isOpen);

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;
    }

    private void UpdateUI()
    {
        foreach (var slot in itemSlots)
        {
            if (slot != null) Destroy(slot);
        }
        itemSlots.Clear();

        if (itemsContainer == null || itemSlotPrefab == null)
            return;

        foreach (var item in items.Values)
        {
            var slot = Instantiate(itemSlotPrefab, itemsContainer);
            itemSlots.Add(slot);

            var iconImage = slot.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null)
            {
                if (item.icon != null)
                {
                    iconImage.sprite = item.icon;
                    iconImage.enabled = true;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }

            var nameText = slot.transform.Find("Name")?.GetComponent<Text>();
            if (nameText != null)
                nameText.text = item.itemName;

            var quantityText = slot.transform.Find("Quantity")?.GetComponent<Text>();
            if (quantityText != null)
                quantityText.text = item.quantity > 1 ? $"x{item.quantity}" : "";
        }
    }

    public int GetItemCount()
    {
        int total = 0;
        foreach (var item in items.Values)
            total += item.quantity;
        return total;
    }
}
