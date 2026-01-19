using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [Header("Key Settings")]
    [SerializeField] private string keyID = "blueKey";
    [SerializeField] private string keyName = "Cheia Albastra";
    [SerializeField] private Sprite keyIcon;

    [Header("Visual")]
    [SerializeField] private GameObject visualObject;
    [SerializeField] private bool rotateKey = true;
    [SerializeField] private float rotationSpeed = 50f;

    [Header("Audio")]
    [SerializeField] private AudioSource pickupAudio;
    [SerializeField] private AudioClip pickupSound;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private bool pickedUp = false;

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
        if (pickedUp) return;

        if (rotateKey)
        {
            if (visualObject != null) visualObject.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            else transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
    }

    public bool CanBePickedUp() => !pickedUp;

    public void PickUpFromInteract()
    {
        if (pickedUp) return;
        pickedUp = true;

        if (debugMode)
        {
            Debug.Log($"[KeyPickup] === Începe ridicarea cheii '{keyID}' ===", this);
        }

        // Găsește TOATE PlayerInventory din scenă
        PlayerInventory[] allInventories = FindObjectsOfType<PlayerInventory>();

        if (debugMode)
        {
            Debug.Log($"[KeyPickup] Găsite {allInventories.Length} PlayerInventory în scenă", this);
        }

        bool addedToInventory = false;

        foreach (PlayerInventory inv in allInventories)
        {
            if (inv != null)
            {
                inv.AddKey(keyID);
                addedToInventory = true;

                if (debugMode)
                {
                    Debug.Log($"[KeyPickup] ✓ Cheia '{keyID}' adăugată în PlayerInventory pe '{inv.gameObject.name}'!", this);
                }
            }
        }

        if (!addedToInventory)
        {
            Debug.LogError("[KeyPickup] ✗ NICIUN PlayerInventory găsit în scenă! Asigură-te că Player are componenta PlayerInventory!", this);
        }

        // Adaugă și în UI Inventory
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.AddItem(keyID, keyName, keyIcon);

            if (debugMode)
            {
                Debug.Log($"[KeyPickup] ✓ Cheia '{keyID}' adăugată în InventorySystem UI!", this);
            }
        }
        else
        {
            Debug.LogWarning("[KeyPickup] InventorySystem.Instance este NULL!", this);
        }

        if (pickupAudio != null && pickupSound != null)
            pickupAudio.PlayOneShot(pickupSound);

        if (visualObject != null) visualObject.SetActive(false);
        else gameObject.SetActive(false);

        Debug.Log($"[KeyPickup] === Cheia '{keyName}' ({keyID}) ridicată cu succes! ===", this);
    }

    public void ResetKey()
    {
        pickedUp = false;

        gameObject.SetActive(true);
        if (visualObject != null)
            visualObject.SetActive(true);

        if (debugMode)
        {
            Debug.Log($"[KeyPickup] Cheia '{keyID}' resetată!", this);
        }
    }
}