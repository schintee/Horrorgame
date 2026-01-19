using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactionRange = 5f;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [Tooltip("Optional: limit what the raycast can hit. Leave as Everything if unsure.")]
    [SerializeField] private LayerMask pickupLayerMask = ~0;

    private PlayerInventory playerInventory;
    private GameObject currentLookingAt;

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // Prefer inventory on the player root (covers the case where collider is on a child)
        playerInventory = GetComponentInParent<PlayerInventory>();
        if (playerInventory == null)
        {
            playerInventory = GetComponent<PlayerInventory>();
        }

        if (playerInventory == null)
        {
            Debug.LogWarning("[PlayerInteraction] No PlayerInventory found on player. Pickups will not grant keys.");
        }
    }

    private void Update()
    {
        CheckForPickup();

        if (Input.GetKeyDown(interactKey) && currentLookingAt != null)
        {
            TryPickup();
        }
    }

    private void CheckForPickup()
    {
        if (playerCamera == null)
        {
            currentLookingAt = null;
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange, pickupLayerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Pickup"))
            {
                currentLookingAt = hit.collider.gameObject;

                PickupItem item = currentLookingAt.GetComponent<PickupItem>();
                if (item != null)
                {
                    // Temporary prompt in Console (replace with UI later)
                    Debug.Log($"Press {interactKey}: {item.GetItemName()}");
                }

                return;
            }
        }

        currentLookingAt = null;
    }

    private void TryPickup()
    {
        if (currentLookingAt == null) return;

        PickupItem item = currentLookingAt.GetComponent<PickupItem>();
        if (item == null) return;

        if (playerInventory == null)
        {
            Debug.LogWarning($"[PlayerInteraction] Tried to pick up '{item.GetItemName()}' but PlayerInventory is missing.");
            return;
        }

        // We treat PickupItem.itemType as a key id (same id used by Door.requiredKey)
        playerInventory.AddKey(item.GetItemType());

        Debug.Log($"Picked up: {item.GetItemName()} (key id: {item.GetItemType()})");

        Destroy(currentLookingAt);
        currentLookingAt = null;
    }

    // Convenience wrappers (optional)
    public bool HasKey(string keyId) => playerInventory != null && playerInventory.HasKey(keyId);

    public void RemoveKey(string keyId)
    {
        if (playerInventory != null)
            playerInventory.RemoveKey(keyId);
    }
}