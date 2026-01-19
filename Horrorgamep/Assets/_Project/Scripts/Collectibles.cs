using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private string itemID = "item_01";
    [SerializeField] private string itemName = "Obiect";
    [SerializeField] private Sprite itemIcon;

    [Header("Visual")]
    [SerializeField] private GameObject visualObject;
    [SerializeField] private bool rotateItem = true;
    [SerializeField] private float rotationSpeed = 50f;

    [Header("Audio")]
    [SerializeField] private AudioSource pickupAudio;
    [SerializeField] private AudioClip pickupSound;

    private bool isCollected = false;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void Update()
    {
        if (isCollected) return;

        if (rotateItem && visualObject != null)
            visualObject.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    public bool CanBePickedUp() => !isCollected;

    public void PickUpFromInteract()
    {
        if (isCollected) return;
        isCollected = true;

        if (InventorySystem.Instance != null)
            InventorySystem.Instance.AddItem(itemID, itemName, itemIcon);

        if (GameManager.Instance != null)
            GameManager.Instance.CollectObject();

        if (pickupAudio != null && pickupSound != null)
            pickupAudio.PlayOneShot(pickupSound);

        if (visualObject != null) visualObject.SetActive(false);
        else gameObject.SetActive(false);

        Debug.Log($"[Collectible] Ai ridicat: {itemName} ({itemID})");
    }

    public void ResetItem()
    {
        isCollected = false;

        gameObject.SetActive(true);
        if (visualObject != null)
            visualObject.SetActive(true);
    }
}
