using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private string requiredItemType = "key";  // Ce item trebuie pentru a deschide
    [SerializeField] private float openDistance = 3f;          // Cât de aproape trebuie să fii
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    private bool isLocked = true;
    private PlayerInteraction playerInteraction;

    void Start()
    {
        // Găsim player-ul și script-ul de interacțiune
        playerInteraction = FindObjectOfType<PlayerInteraction>();
    }

    void Update()
    {
        if (!isLocked) return;  // Dacă e deschisă, nu mai face nimic

        // Verificăm dacă player-ul e aproape
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance <= openDistance)
            {
                // Verificăm dacă player-ul are cheia
                if (playerInteraction != null && playerInteraction.HasItem(requiredItemType))
                {
                    Debug.Log("Press E to unlock door");

                    if (Input.GetKeyDown(interactionKey))
                    {
                        UnlockDoor();
                    }
                }
                else
                {
                    Debug.Log("Door is locked. You need a key!");
                }
            }
        }
    }

    void UnlockDoor()
    {
        isLocked = false;

        // Consumă cheia
        playerInteraction.UseItem(requiredItemType);

        Debug.Log("Door unlocked!");

        // Animație simplă - ușa dispare
        // (mai târziu poți face animație de deschidere)
        Destroy(gameObject);
    }
}