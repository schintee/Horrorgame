using UnityEngine;
using UnityEngine.Events;

public class Collectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    [SerializeField] private string itemName = "Item";
    [SerializeField] private GameObject visualObject;

    [Header("Audio")]
    [SerializeField] private AudioSource collectAudio;
    [SerializeField] private AudioClip collectSound;

    public UnityEvent OnCollected;

    private bool isCollected = false;

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        isCollected = true;

        if (collectAudio != null && collectSound != null)
        {
            collectAudio.PlayOneShot(collectSound);
        }

        OnCollected?.Invoke();

        // Hide visual
        if (visualObject != null)
        {
            visualObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void ResetCollectible()
    {
        isCollected = false;

        if (visualObject != null)
        {
            visualObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}
