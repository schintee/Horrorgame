// Key Pickup
using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [Header("Key Settings")]
    [SerializeField] private string keyID;
    [SerializeField] private GameObject visualObject;

    [Header("Audio")]
    [SerializeField] private AudioSource pickupAudio;
    [SerializeField] private AudioClip pickupSound;

    private bool isPickedUp = false;

    void OnTriggerEnter(Collider other)
    {
        if (isPickedUp) return;

        if (other.CompareTag("Player"))
        {
            PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
            if (inventory == null)
            {
                Debug.LogWarning($"[KeyPickup] No PlayerInventory found on '{other.name}' (or its parents). Cannot pick up key '{keyID}'.");
                return;
            }

            inventory.AddKey(keyID);

            if (pickupAudio != null && pickupSound != null)
            {
                pickupAudio.PlayOneShot(pickupSound);
            }

            isPickedUp = true;

            if (visualObject != null)
            {
                visualObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
