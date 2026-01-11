using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public string itemName = "Item";
    public string itemType = "generic";

    public string GetItemName() { return itemName; }
    public string GetItemType() { return itemType; }

    void Update()
    {
        transform.Rotate(Vector3.up * 50f * Time.deltaTime);
    }
}