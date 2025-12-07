using UnityEngine;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f;  // Cât de departe po?i interac?iona
    [SerializeField] private Camera playerCamera;          // Camera player-ului
    [SerializeField] private KeyCode interactionKey = KeyCode.E;  // Tasta pentru pickup

    // Inventar simplu - list? de obiecte
    private List<string> inventory = new List<string>();

    // Obiectul pe care ne uit?m acum
    private GameObject currentLookingAt;

    void Start()
    {
        // G?sim camera automat dac? nu e setat?
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }
    }

    void Update()
    {
        // Verific?m dac? ne uit?m la un obiect pickup
        CheckForPickup();

        // Dac? ap?s?m E ?i ne uit?m la ceva
        if (Input.GetKeyDown(interactionKey) && currentLookingAt != null)
        {
            TryPickup();
        }

        // Debug - vezi inventarul cu I
        if (Input.GetKeyDown(KeyCode.I))
        {
            ShowInventory();
        }
    }

    void CheckForPickup()
    {
        // Raycast din mijlocul ecranului
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            // Verific?m dac? obiectul are tag-ul Pickup
            if (hit.collider.CompareTag("Pickup"))
            {
                currentLookingAt = hit.collider.gameObject;

                // Afi??m în console (mai târziu facem UI)
                PickupItem item = currentLookingAt.GetComponent<PickupItem>();
                if (item != null)
                {
                    Debug.Log("Press E to pick up: " + item.GetItemName());
                }
            }
            else
            {
                currentLookingAt = null;
            }
        }
        else
        {
            currentLookingAt = null;
        }
    }

    void TryPickup()
    {
        PickupItem item = currentLookingAt.GetComponent<PickupItem>();
        if (item != null)
        {
            // Adaug? în inventar
            string itemName = item.GetItemName();
            string itemType = item.GetItemType();

            inventory.Add(itemType);  // Salv?m tipul pentru puzzle-uri

            Debug.Log("Picked up: " + itemName);

            // Distruge obiectul din scen?
            Destroy(currentLookingAt);
            currentLookingAt = null;
        }
    }

    void ShowInventory()
    {
        Debug.Log("=== INVENTORY ===");
        if (inventory.Count == 0)
        {
            Debug.Log("Empty");
        }
        else
        {
            foreach (string item in inventory)
            {
                Debug.Log("- " + item);
            }
        }
    }

    // Func?ie public? pentru puzzle-uri - verific? dac? ai un item
    public bool HasItem(string itemType)
    {
        return inventory.Contains(itemType);
    }

    // Func?ie pentru a folosi/consuma un item
    public void UseItem(string itemType)
    {
        if (inventory.Contains(itemType))
        {
            inventory.Remove(itemType);
            Debug.Log("Used item: " + itemType);
        }
    }
}