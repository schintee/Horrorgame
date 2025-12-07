using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private string itemName = "Item";      // Numele obiectului (Key, Battery, etc)
    [SerializeField] private string itemType = "generic";   // Tipul: key, battery, note, etc

    // Funcție publică ca să putem lua informații despre item
    public string GetItemName()
    {
        return itemName;
    }

    public string GetItemType()
    {
        return itemType;
    }

    // Opțional: rotație pentru a face item-ul mai vizibil
    void Update()
    {
        // Rotește încet obiectul (ca în jocuri)
        transform.Rotate(Vector3.up * 50f * Time.deltaTime);
    }
}
