// Interaction System
using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("UI")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private UnityEngine.UI.Text promptText;

    private Camera playerCamera;
    private Door currentDoor;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        HidePrompt();
    }

    void Update()
    {
        CheckForInteractable();

        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    private void CheckForInteractable()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
        {
            // IMPORTANT: colliderul lovit poate fi pe un copil, iar Door poate fi pe parinte
            Door door = hit.collider.GetComponentInParent<Door>();

            if (door != null)
            {
                currentDoor = door;
                ShowPrompt(door.IsLocked ? "Locked" : (door.IsOpen ? "Close [E]" : "Open [E]"));
                return;
            }
        }

        currentDoor = null;
        HidePrompt();
    }

    private void TryInteract()
    {
        if (currentDoor != null)
        {
            currentDoor.Interact();
        }
    }

    private void ShowPrompt(string text)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);

            if (promptText != null)
            {
                promptText.text = text;
            }
        }
    }

    private void HidePrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
}
